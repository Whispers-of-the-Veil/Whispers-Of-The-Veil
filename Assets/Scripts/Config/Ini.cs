// Lucas

using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Config {
    public class Ini {
        private string fileName;
        private static string path;

        public enum Sections {
            API,
        }
        
        public enum Keys {
            port,
            address,
        }

        public Ini(string fileName) {
            this.fileName = fileName;
            
            if (!GetPath()) {
                throw new UnityException("Ini file doesnt exist; cannot initialize");
            }
        }

        /// <summary>
        /// Gets the file path of the config.ini file based on if we are in the editor or in the build
        /// </summary>
        /// <returns>If the file exists, return true; false otherwise</returns>
        private bool GetPath() {
            // Get the path of the executable, both for the editor and build
            if (Application.isEditor) {
                // Poits inside the assets folder
                path = Application.dataPath + "/Scripts/SpeechRecognition/Src/dist/" + this.fileName;
            }
            else {
                // For Windows and Linux machines, this points to the Data folder
                // For MacOS, this points to the contents folder within the app package
                path = Application.dataPath + "/" + this.fileName;
            }
            
            Debug.Log(path);

            if (!File.Exists(path)) {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Gets the value of a key within a section of the ini file
        /// </summary>
        /// <param name="section">A section from the enum</param>
        /// <param name="key">A key from the enum</param>
        /// <returns>A string of the value found; null if the key didn't exist</returns>
        public string GetValue(Sections section, Keys key) {
            // Get the contents of the file
            string[] contents = File.ReadAllLines(path);
            bool inSection = false;

            foreach (var line in contents) {
                if (!inSection) {
                    // Check if our line contains our desired section
                    if (line.Equals("[" + section + "]")) {
                        inSection = true;
                    }
                }
                else {
                    // If the line contains a [, we are in a new section and we can stop the loop
                    // If our line doesnt contain the key, continue to next iteration
                    if (line.Contains("[")) {
                        break;
                    }
                    else if (!line.Contains(key.ToString())) {
                        continue;
                    }
                    
                    var keyValue = line.Split('=');

                    return keyValue[1];
                }
            }

            // Return null if we weren't able to fine the key
            return null;
        }
    }
}