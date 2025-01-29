from abc import ABC, abstractmethod

class Interface(ABC):
    @abstractmethod
    def Predict(self, value):
        """
        Generate a prediction from a given model
        
        Parameters:
            - value: np.float23

        Returns:
        A string representing the prediction
        """
        pass