# import language_tool_python

class NLP():
    def __init__(self):
        pass
#         self.tool = language_tool_python.LanguageTool('en-US')

    def SpellingCorrection(self, _prediction):
        """
        This function will preform spelling correction to the models
        prediction using the language_tool_python's method correct()

        Parameter:
            - _prediction: The models prediction

        Returns:
            The prediction with known words spelling corrected.
        """
#         corrected = self.tool.correct(_prediction)

        return _prediction