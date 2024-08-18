# Patient Study Records Application

This is a WPF .NET Core 8.0 application designed to retrieve and manage patient study records from the Eigen Profuse database. 
The application allows users to filter patient records by year and month, view them in a DataGrid, and export the data to CSV or XLSX formats. 
Additionally, it includes features for copying individual cell contents to the clipboard.

## Features

- **Filter Patient Records**: Filter records by selecting a specific year and month.
- **"All Month" Option**: Option to view all records for a selected year by checking the "All month" checkbox.
- **Export Data**: Export filtered patient records to CSV or XLSX files.
- **Copy Cell Content**: Right-click on any cell to copy its content to the clipboard.
- **Responsive UI**: The application UI is responsive, resizing controls appropriately as the window is resized.

## Database

This application connects to the Eigen Profuse database configured in the appsettings.json. 
It retrieves patient study records stored in the `patients` table, with the following columns:

- `RecDateTime` - Record Date and Time
- `TimeLastUpdate` - Last Update Time
- `PatDicom` - Patient DICOM ID
- `PatDOB` - Patient Date of Birth
- `PatID` - Patient ID
- `PatGender` - Patient Gender
- `PatAge` - Patient Age
- `PatWeight` - Patient Weight
- `PatComments` - Patient Comments

## Getting Started

### Prerequisites

- .NET Core 8.0 SDK
- MariaDB server with access to the Eigen Profuse database

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/itschris/patient-study-records.git
   cd patient-study-records
   ```

2. Restore dependencies:

   ```bash
   dotnet restore
   ```

3. Build the application:

   ```bash
   dotnet build
   ```

4. Run the application:

   ```bash
   dotnet run
   ```

### Configuration

Database connection properties are stored in `appsettings.json`. Modify this file to configure the connection string:

```json
{
  "ConnectionStrings": {
    "MariaDB": "Server=servername;Port=3306;Database=profuse;User Id=root;Password=YourPassword;"
  }
}
```

### Usage

1. **Filtering Records**: Select a year from the dropdown. The month dropdown will populate with available months. Select a month or check "All month" to view all records for the year.

2. **Exporting Data**: After loading records, click "Export to CSV" or "Export to XLSX" to save the data.

3. **Copying Cell Content**: Right-click on any cell in the DataGrid and select "Copy Cell Content" to copy the content to the clipboard.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [EPPlus](https://github.com/EPPlusSoftware/EPPlus) for XLSX export functionality.
- [MariaDB](https://mariadb.org/) for database support.
```

### Additional Notes:
- Replace `"yourusername"` in the `git clone` URL with your actual GitHub username.
- Customize any sections with specific details about your project or environment as needed.

This `README.md` provides a clear overview of your project, its features, installation instructions, and usage details. Let me know if there's anything more you'd like to add or modify!