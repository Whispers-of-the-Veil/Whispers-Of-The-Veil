from abc import ABC, abstractmethod

class Interface(ABC):
    def __init__(self):
        super().__init__()

    @abstractmethod
    def Predict(self, _value):
        """
        Generate a prediction from a given model
        
        Parameters:
            - _value: np.float23

        Returns:
        A string representing the prediction
        """
        pass

    @abstractmethod
    def PostProcess(self, _prediction):
        """
        Preform post processing on the models prediction to clean up the models
        output.

        Parameters:
            - _prediction: A string of the prediction generated from the model

        returns:
            The cleaned up prediction
        """
        pass