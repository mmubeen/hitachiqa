# Load the Windows Forms assembly
Add-Type -AssemblyName System.Windows.Forms

# Define the form
$form = New-Object System.Windows.Forms.Form
$form.Text = "PowerShell Form"
$form.ClientSize = New-Object System.Drawing.Size(400, 180)
$form.FormBorderStyle = [System.Windows.Forms.FormBorderStyle]::FixedDialog
$form.MaximizeBox = $false
$form.MinimizeBox = $false
$form.StartPosition = [System.Windows.Forms.FormStartPosition]::CenterScreen

# Define the form elements
$projectNameLabel = New-Object System.Windows.Forms.Label
$projectNameLabel.Location = New-Object System.Drawing.Point(10, 20)
$projectNameLabel.Size = New-Object System.Drawing.Size(100, 20)
$projectNameLabel.Text = "Project Name:"
$form.Controls.Add($projectNameLabel)

$projectNameTextBox = New-Object System.Windows.Forms.TextBox
$projectNameTextBox.Location = New-Object System.Drawing.Point(120, 20)
$projectNameTextBox.Size = New-Object System.Drawing.Size(160, 20)
$form.Controls.Add($projectNameTextBox)

$hostLabel = New-Object System.Windows.Forms.Label
$hostLabel.Location = New-Object System.Drawing.Point(10, 50)
$hostLabel.Size = New-Object System.Drawing.Size(100, 20)
$hostLabel.Text = "Host:"
$form.Controls.Add($hostLabel)

$hostTextBox = New-Object System.Windows.Forms.TextBox
$hostTextBox.Location = New-Object System.Drawing.Point(120, 50)
$hostTextBox.Size = New-Object System.Drawing.Size(160, 20)
$hostTextBox.Text = "https://www.hitachi.us"
$form.Controls.Add($hostTextBox)

$targetFrameworkLabel = New-Object System.Windows.Forms.Label
$targetFrameworkLabel.Location = New-Object System.Drawing.Point(10, 80)
$targetFrameworkLabel.Size = New-Object System.Drawing.Size(100, 20)
$targetFrameworkLabel.Text = "Target Framework:"
$form.Controls.Add($targetFrameworkLabel)

$targetFrameworkComboBox = New-Object System.Windows.Forms.ComboBox
$targetFrameworkComboBox.Location = New-Object System.Drawing.Point(120, 80)
$targetFrameworkComboBox.Size = New-Object System.Drawing.Size(160, 20)
$targetFrameworkComboBox.DropDownStyle = [System.Windows.Forms.ComboBoxStyle]::DropDownList
$targetFrameworkComboBox.Items.AddRange(@("net6.0", "net7.0"))
$targetFrameworkComboBox.SelectedIndex = 0
$form.Controls.Add($targetFrameworkComboBox)

$outputFolderLabel = New-Object System.Windows.Forms.Label
$outputFolderLabel.Location = New-Object System.Drawing.Point(10, 110)
$outputFolderLabel.Size = New-Object System.Drawing.Size(100, 20)
$outputFolderLabel.Text = "Output Folder:"
$form.Controls.Add($outputFolderLabel)

$outputFolderTextBox = New-Object System.Windows.Forms.TextBox
$outputFolderTextBox.Location = New-Object System.Drawing.Point(120, 110)
$outputFolderTextBox.Size = New-Object System.Drawing.Size(200, 20)
$outputFolderTextBox.Text = $pwd
$form.Controls.Add($outputFolderTextBox)

$chooseFolderButton = New-Object System.Windows.Forms.Button
$chooseFolderButton.Location = New-Object System.Drawing.Point(330, 110)
$chooseFolderButton.Size = New-Object System.Drawing.Size(70, 20)
$chooseFolderButton.Text = "choose..."
$chooseFolderButton.Add_Click({
    $folderDialog = New-Object System.Windows.Forms.FolderBrowserDialog
    $folderDialog.ShowDialog() | Out-Null
    $outputFolderTextBox.Text = $folderDialog.SelectedPath
})
$form.Controls.Add($chooseFolderButton)


$createButton = New-Object System.Windows.Forms.Button
$createButton.Location = New-Object System.Drawing.Point(100, 140)
$createButton.Size = New-Object System.Drawing.Size(100, 23)
$createButton.Text = "Create"
$createButton.DialogResult = [System.Windows.Forms.DialogResult]::OK
$form.AcceptButton = $createButton
$form.Controls.Add($createButton)

# Show the form and prompt the user
$result = $form.ShowDialog()

# If the user clicked the Run button, continue with the script
if ($result -eq [System.Windows.Forms.DialogResult]::OK) {
    $projectName = $projectNameTextBox.Text
    $TargetHost = $hostTextBox.Text
    $targetFramework = $targetFrameworkComboBox.SelectedItem.ToString()

    if ($TargetHost -notlike "https://*") {
        $TargetHost = "https://$TargetHost"
    }

    Invoke-Expression -Command ".\createSolution.ps1 -projectName $projectName -targetFramework $targetFramework -targetHost $targetHost"
}

