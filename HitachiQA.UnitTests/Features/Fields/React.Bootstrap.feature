@NoBrowser
Feature: React.Bootstrap

A short summary of the feature


Scenario: Test SetFieldValue
	Given The HTML page is loaded for 'react.bootstrap'
	When User sets the value '<FieldValue>' for the field '<FieldIdentifier>'
	Then The field '<FieldIdentifier>' value should be '<ExpectedValue>' or '<FieldValue>'
	And attach video

	Examples: 
	| description                           | FieldIdentifier     | FieldValue   | ExpectedValue |
	
	#button dropdown
	| Button_Dropdown_By_Input_Id           | dropdownMenuButton  | Action 1     |               |
	| Button_Dropdown_By_Label              | Dropdown of Buttons | Action 2     |               |
	| Button_Dropdown_By_Parent_Id          | button-dropdown     | Action 3     |               |

	#Checkbox buttons
	| Checkbox_By_Parent_Id                 | checkboxes          | Checkbox 1   |               |
	| Checkbox_By_Parent_Label              | Checkboxes          | Checkbox 2   |               |

	#dropdown
	| Dropdown_By_Input_Id_Option_By_Text   | dropdown            | Option 1     |               |
	| Dropdown_By_Input_Id_Option_By_Value  | dropdown            | 2            | Option 2      |
	| Dropdown_By_Label_Option_By_Text      | Dropdown            | Option 1     |               |
	| Dropdown_By_Label_Option_By_Value     | Dropdown            | 2            | Option 2      |
	| Dropdown_By_Parent_Id_Option_By_Text  | basic-dropdown      | Option 1     |               |
	| Dropdown_By_Parent_Id_Option_By_Value | basic-dropdown      | 2            | Option 2      |

	
	#number only textfield
	| NumberField_By_Input_Id               | numberInput         | 10           |               |
	| NumberField_By_Parent_Id              | numeric-input       | 20           |               |
	| NumberField_By_Label                  | Numeric Input       | 30           |               |

	#radio buttons
	| RadioButton_By_Parent_Id              | radio-buttons       | Option 1     |               |
	| RadioButton_By_Parent_Label           | Radio Buttons       | Option 2     |               |

	#textfield
	| TextField_By_Input_Id                 | textInput           | By Input Id  |               |
	| TextField_By_Parent_Id                | text-input          | By Parent Id |               |
	| TextField_By_Label                    | Text Input          | By Text      |               |


Scenario: Test SetFieldValue_RadioButton_Click_By_Value_Label
	Given The HTML page is loaded for 'react.bootstrap'
	When User clicks radio button with label 'Option 3'
	Then The field 'radio-buttons' value should be 'Option 3' or ''
	And attach video

Scenario: Test SetFieldValue_Checkbox_Set_Multiple_Values_Sequential
	Given The HTML page is loaded for 'react.bootstrap'
	When User sets the value 'Checkbox 1' for the field 'checkboxes'
	And User sets the value 'Checkbox 2' for the field 'checkboxes'
	Then The field 'checkboxes' value should be 'Checkbox 1;Checkbox 2' or ''
	And attach video

Scenario: Test SetFieldValue_Checkbox_Set_Multiple_Values_SemiColon
	Given The HTML page is loaded for 'react.bootstrap'
	When User sets the value 'Checkbox 1;Checkbox 2' for the field 'checkboxes'
	Then The field 'checkboxes' value should be 'Checkbox 1;Checkbox 2' or ''
	And attach video


Scenario: Test SetFieldValue_Checkbox_Set_Multiple_Values_Array
	Given The HTML page is loaded for 'react.bootstrap'
	When User sets the value 'Checkbox 1;Checkbox 2' for the field 'checkboxes' as array
	Then The field 'checkboxes' value should be 'Checkbox 1;Checkbox 2' or ''
	And attach video