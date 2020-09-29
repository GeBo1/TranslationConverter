# TranslationConverter
 
CLI Application meant for dealing with conversion of translationfiles for https://github.com/bbepis/XUnity.AutoTranslator.

Application options:

### Converting CSV Files to XUA Compatible TXTs

This option takes a existing CSV file previously prepared from this app, and makes it into a XUA compatible folder structure and file setup.

#### Requirement

Option needs either passed csv file or the existence of csvfile.csv in the exe dir.
csv should have 3 culumns; Original text, Translated text, Translation Notes

#### Passable
This option takes a csv file passed as argument.

#### Failout
If there is no file passed, it will attempt to run the operation on a file in the exe folder named csvfile.csv.

#### Result
This option will output to a folder named 1translation in the exe dir.
This option will also create a master.txt with all unique lines from the h folder for use in the next step.

### Check and populate duplicate h lines

This option will check the output from the csv in the above option, check for uniques

#### Requirement
The 1translation folder from the above option.
The master.txt from the above option.

#### Passable
N/A

#### Failout
N/A

#### Result
This option will output to a folder named 2translation in the exe dir.

### Cleanup translation for release

This option runs formatting cleanup on the result of the above action.

#### Requirement
The 2translation or 1translation folder from the above options.

#### Passable
N/A

#### Failout
If the 2translation folder doesn't exist, fails back to 1translation.

#### Result
This option will output to a folder named 3translation in the exe dir.

### Convert XUA folder to CSV

#### Requirement
A correctly formatted abdata XUA dir.

#### Passable
folder path.

#### Failout
If abdata folder isn't passed, will attempt to use abdata folder in exe dir.

#### Result
Will generate a folder named CSVFiles in the exe dir and populate it with csv files split by character.

### Turn h duplicate checked folder back into CSV
Will turn the working folder for H Dupe Check back into a CSV file.

#### Requirement
2translation folder from previous steps.

#### Passable
N/A

#### Failout
N/A

#### Result
newTranslation.csv file in exe folder.

### Turn cleanuped folder back into CSV

#### Requirement
3translation folder from previous steps.

#### Passable
N/A

#### Failout
N/A

#### Result
newTranslation.csv file in exe folder.

### Create dupechecked and cleaned CSVs
Takes a correctly formatted CSV, checks for duplicate entries in the h translation, populates duplicates and proceeds to run a formatting cleanup.

#### Requirement
Option needs the existence of csvfile.csv in the exe dir.
csv should have 3 culumns; Original text, Translated text, Translation Notes

#### Passable
N/A

#### Failout
N/A

#### Result
Treated_Cleaned.csv and Treated_DupeChecked.csv in the exe dir.
