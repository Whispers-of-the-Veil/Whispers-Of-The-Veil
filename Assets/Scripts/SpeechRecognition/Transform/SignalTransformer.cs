using System;
using System.Linq;
using UnityEngine;

namespace SpeechRecognition.Transform {
    public class SignalTransformer {
        /// <summary>
        /// This method will generate a hanning window using the following formula:
        ///     W[n] = 0.5 * (1.0 - cos(2π * n/N))
        ///
        /// This window will be used to segment the audio sample into overlaping frames
        /// </summary>
        /// <param name="N">This is the length of the window</param>
        /// <returns>A float array holding the windows weights</returns>
        private float[] CreateHanningWindow(int N) {
            float[] window = new float[N];

            for (int i = 0; i < N; i++) {
                window[i] = 0.5f * (1.0f - Mathf.Cos(2.0f * Mathf.PI * i / (N - 1)));
            }

            return window;
        }
        
        /// <summary>
        /// This method follows the Cooley-Tukey algorithm. And was derived from the implementation in the book "Numerical Recipes in C".
        /// It was changed to utilize the ComplexValues structure and to include index 0. It was also modified to include some
        /// exception cases.
        /// This method will preform the transform in place with the value array.
        /// We are following these equations:
        ///     F(u) = Σ_{x=0}^{N-1}f(x)e^{(-j2πux)/N} (forward)
        ///     f(x) = Σ_{u=0}^{N-1}F(u)e^{(j2πux)/N} (inverse)
        ///
        /// Make sure to apply the normalization method to the forward transform
        /// </summary>
        /// <param name="value">These are the values of the frame segment</param>
        /// <param name="N">This is the size of the frame</param>
        /// <param name="isign">This sign indicates if we are preforming the inverse or forward transform: -1 (forward); 1(inverse)</param>
        private int FFT(ComplexValues[] value, int N, int isign) {
            int mmax, m, j = 0, istep, i;
            double theta, wtemp, wr, wpr, wi;
            float tempr, tempi;

            try {
                if ((N & (N - 1)) != 0) {
                    throw new ArgumentException("Invalid value; segment length needs to be a power of 2");
                }

                // Bit-reversal permutation
                for (i = 0; i < N; i++) {
                    if (j > i) {
                        SWAP(ref value[j].real, ref value[i].real);
                        SWAP(ref value[j].imag, ref value[i].imag);
                    }

                    m = N / 2;
                    while (j >= m && m >= 1) {
                        j -= m;
                        m /= 2;
                    }

                    j += m;
                }

                // Main FFT computation
                mmax = 1;
                while (N > mmax) {
                    istep = mmax << 1;
                    theta = isign * (2.0 * Math.PI / mmax);
                    wtemp = Math.Sin(0.5 * theta);
                    wpr = -2.0 * wtemp * wtemp;
                    wr = 1.0;
                    wi = 0.0;

                    for (m = 0; m < mmax; m++) {
                        for (i = m; i < N; i += istep) {
                            j = i + mmax;

                            if (j >= N) {
                                Debug.Log($"Index out of range: j = {j}, n = {N}");
                                String message =
                                    "Please change the segment length to be a power of 2 that is within the length of the frame...";
                                throw new IndexOutOfRangeException(message);
                            }

                            tempr = (float)(wr * value[j].real - wi * value[j].imag);
                            tempi = (float)(wr * value[j].imag + wi * value[j].real);

                            value[j].real = value[i].real - tempr;
                            value[j].imag = value[i].imag - tempi;

                            value[i].real += tempr;
                            value[i].imag += tempi;
                        }

                        wtemp = wr;
                        wr = wr * wpr - wi * Math.Sin(theta) + wr;
                        wi = wi * wpr + wtemp * Math.Sin(theta) + wi;
                    }

                    mmax = istep;
                }
            }
            catch (IndexOutOfRangeException ex) {
                Debug.Log(ex.Message);
                return 1;
            }
            catch (ArgumentException ex) {
                Debug.Log(ex.Message);
                return 1;
            }
            catch (Exception ex) {
                Debug.Log(ex.Message);
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// This method is used to normalize the forward FFT of a frame. It will preform
        /// this operation in place.
        /// </summary>
        /// <param name="values">These are the values of the Forward FFT</param>
        /// <param name="N">This is the size of the frame</param>
        private void NormalizeFFT(ComplexValues[] values, int N) {
            for (int i = 0; i < N; i++) {
                values[i].real /= N;
                values[i].imag /= N;
            }
        }
        
        /// <summary>
        /// This method simply swaps the arguments
        /// </summary>
        /// <param name="a">Value to swap</param>
        /// <param name="b">Vlaue to swape</param>
        private void SWAP(ref float a, ref float b) {
            float temp = a;
            a = b;
            b = temp;
        }

        public ComplexValues BackToComplex(float magnitude, float phase) {
            ComplexValues values = new ComplexValues();

            values.real = magnitude * Mathf.Cos(phase);
            values.imag = magnitude * Mathf.Sin(phase);

            return values;
        }

        public float Magnitude(ComplexValues values) {
            return Mathf.Sqrt(values.real * values.real + values.imag * values.imag);
        }

        public float Phase(ComplexValues values) {
            return Mathf.Atan(values.imag / values.real);
        }

        /// <summary>
        /// FORWARD: This method the short-time fourier transform of an input 1D signal. It will apply
        /// the hanning window to the audio singal to segment it into different frames, then apply the
        /// FFT mehtod to each frame normalizing the results with 1/N.
        /// This mehtod follows this equation:
        ///     X(m, w) = Σ_{-∞}^{∞} x[n]w[n-m]e^{-iwn}
        /// This method is overloaded, if you pass a float array it will compute the forward transform, if you
        /// pass the 2D ComplexValues matrix it will compute the inverse transform.
        /// </summary>
        /// <param name="signal">A float array containing the signal original data</param>
        /// <param name="segmentLength">This is the length of each frame / segement that the FFT will be preformed on</param>
        /// <param name="hopSize">Determines the amount of overlap of each frame (should be a value bellow the seglength)</param>
        /// <returns>A 2D ComplexValues array containing the real and imagenary values of the fourier transform</returns>
        public ComplexValues[][] stft(float[] signal, int segmentLength, int hopSize) {
            int numSegments = 1 + (signal.Length - segmentLength) / hopSize;
            float[] window = CreateHanningWindow(segmentLength);
            float[] pitches = new float[numSegments];
            float avgPitch;
            
            ComplexValues[][] stft = new ComplexValues[numSegments][];
            
            // For each frame
            for (int i = 0; i < numSegments; i++) {
                ComplexValues[] frame = new ComplexValues[segmentLength];
                
                // Apply the window function and shift the frequencies to the center
                for (int j = 0; j < segmentLength; j++) {
                    frame[j] = new ComplexValues {
                        real = signal[(i * hopSize) + j] * window[j] * Mathf.Pow(-1, j),
                        imag = 0
                    };
                }

                // Preform the forward FFT on the frame
                if (FFT(frame, segmentLength, -1) != 0) {
                    return null;
                }
                NormalizeFFT(frame, segmentLength);
                
                stft[i] = frame;
            }

            return stft;
        }
        
        /// <summary>
        /// INVERSE. This method utilizes overlap-add to add together the real values of the inverse FFT method; discarding
        /// the imaginary values.
        /// this method follows this formula:
        ///     x[n] = Σ_{m=0}^{M-1} x[n]X[M,k]*w[n-m]
        /// This method is overloaded, if you pass a float array it will compute the forward transform, if you
        /// pass the 2D ComplexValues matrix it will compute the inverse transform.
        /// </summary>
        /// <param name="values">A 2D ComplexValues matrix of real and imaginary values</param>
        /// <param name="segmentLength">This is the length of each frame / segement that the FFT will be preformed on</param>
        /// <param name="hopSize">Determines the amount of overlap of each frame (should be a value bellow the seglength)</param>
        /// <returns>A float array of the signal values</returns>
        public float[] stft(ComplexValues[][] values, int segmentLength, int hopSize) {
            int length = (values.Length - 1) * hopSize + segmentLength;
            
            float[] signal = new float[length];
            float[] window = CreateHanningWindow(segmentLength);
            
            float[] normalizationFactor = new float[length];

            // for each frame
            for (int i = 0; i < values.Length; i++) {
                ComplexValues[] frame = values[i];

                // Perform inverse FFT
                if (FFT(frame, segmentLength, 1) != 0) {
                    return null;
                }

                // Overlap-add and apply window function, we are also undoing the shift here
                for (int j = 0; j < segmentLength; j++) {
                    int signalIndex = i * hopSize + j;
                    signal[signalIndex] += frame[j].real * window[j] * Mathf.Pow(-1, j);
                    normalizationFactor[signalIndex] += window[j] * window[j];
                }
            }

            // Normalize the signal to correct for overlapping window contributions
            for (int i = 0; i < length; i++) {
                if (normalizationFactor[i] > 0) {
                    signal[i] /= normalizationFactor[i];
                }
            }

            return signal;
        }
    }
}