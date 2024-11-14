from configparser import ConfigParser

class ini:
    def grabInfo(self, filename: str, section: str) -> dict:
        """
        Reads in the config file, taps into the section, and retrieves the key-value pairs.

        Parameter:
            - filename: the name of the file that hold the connection information
            - section: the section of the ini file that contains the connection information
        """
        # instantiating the parser object
        parser = ConfigParser()
        parser.read(filename)

        db_info = {}

        if parser.has_section(section):
            # items() method returns (key, value) tuples
            key_val_tuple = parser.items(section) 

            for item in key_val_tuple:
                db_info[item[0]] = item[1] # index 0: key & index 1: value

        return db_info