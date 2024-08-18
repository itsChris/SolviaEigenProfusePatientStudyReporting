using OfficeOpenXml;
using Serilog;
using SolviaEigenProfusePatientStudyReporting.Models;
using SolviaEigenProfusePatientStudyReporting.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SolviaEigenProfusePatientStudyReporting.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        public ICommand ExportToXlsxCommand { get; }
        public ICommand OpenLogFileCommand { get; }
        public ICommand LoadPatientsCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ObservableCollection<int> AvailableYears { get; set; }
        public ObservableCollection<int> AvailableMonths { get; set; }
        public ObservableCollection<Patient> Patients { get; set; }

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    LoadAvailableMonthsAsync();
                    UpdateMonthComboBoxState();
                }
            }
        }

        private int _selectedMonth;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value))
                {
                    IsLoadPatientsEnabled = SelectedYear > 0 && !IsAllMonthsSelected; // Enable Load Patients if both Year and Month are selected and "All month" is not checked
                }
            }
        }

        private bool _isAllMonthsSelected;
        public bool IsAllMonthsSelected
        {
            get => _isAllMonthsSelected;
            set
            {
                if (SetProperty(ref _isAllMonthsSelected, value))
                {
                    UpdateMonthComboBoxState();
                    IsLoadPatientsEnabled = value || SelectedYear > 0 && SelectedMonth > 0; // Enable Load Patients if "All month" is selected or if both Year and Month are selected
                }
            }
        }

        private bool _isMonthEnabled = false;
        public bool IsMonthEnabled
        {
            get => _isMonthEnabled;
            private set => SetProperty(ref _isMonthEnabled, value);
        }

        private bool _isLoadPatientsEnabled = false;
        public bool IsLoadPatientsEnabled
        {
            get => _isLoadPatientsEnabled;
            private set => SetProperty(ref _isLoadPatientsEnabled, value);
        }

        private bool _isExportEnabled = false;
        public bool IsExportEnabled
        {
            get => _isExportEnabled;
            private set => SetProperty(ref _isExportEnabled, value);
        }
        private void UpdateMonthComboBoxState()
        {
            IsMonthEnabled = SelectedYear > 0 && !IsAllMonthsSelected; // Enable Month ComboBox only if a year is selected and "All month" is not checked
        }



        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Patients = new ObservableCollection<Patient>();
            AvailableYears = new ObservableCollection<int>();
            AvailableMonths = new ObservableCollection<int>();

            LoadPatientsCommand = new RelayCommand(async () => await LoadPatientsAsync(), () => IsLoadPatientsEnabled);
            ExportToCsvCommand = new RelayCommand(ExportToCsv, () => IsExportEnabled);
            OpenLogFileCommand = new RelayCommand(OpenLogFile);

            LoadAvailableYearsAsync();

            ExportToXlsxCommand = new RelayCommand(ExportToXlsx, () => IsExportEnabled);

        }

        private void OpenLogFile()
        {
            try
            {
                Log.Verbose("OpenLogFile command executed.");

                // Adjust the path if necessary
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

                // Verify the log directory path
                Log.Verbose("Looking for log files in directory: {LogDirectory}", logDirectory);

                if (!Directory.Exists(logDirectory))
                {
                    Log.Warning("Log directory does not exist: {LogDirectory}", logDirectory);
                    MessageBox.Show("The log directory does not exist.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string[] logFiles = Directory.GetFiles(logDirectory, "*.log");

                if (logFiles.Length > 0)
                {
                    // Get the latest log file by last write time
                    string latestLogFile = logFiles.OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = latestLogFile,
                        UseShellExecute = true
                    });

                    Log.Information("Opened log file: {LogFile}", latestLogFile);
                }
                else
                {
                    Log.Warning("No log files found in the log directory: {LogDirectory}", logDirectory);
                    MessageBox.Show("No log files found.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while trying to open the log file.");
                MessageBox.Show("An error occurred while trying to open the log file. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void LoadAvailableYearsAsync()
        {
            try
            {
                Log.Verbose("LoadAvailableYearsAsync initiated.");

                var years = await _databaseService.GetAvailableYearsAsync();
                AvailableYears.Clear();

                foreach (var year in years)
                {
                    AvailableYears.Add(year);
                    Log.Verbose("Added year to AvailableYears collection: {Year}", year);
                }

                Log.Information("LoadAvailableYearsAsync completed. Years loaded: {YearCount}", AvailableYears.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while loading available years.");
                MessageBox.Show("An error occurred while loading the available years. Please try again.", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void LoadAvailableMonthsAsync()
        {
            try
            {
                Log.Verbose("LoadAvailableMonthsAsync initiated for year: {SelectedYear}", SelectedYear);

                IsLoadPatientsEnabled = false; // Disable Load Patients button until both Year and Month are selected
                IsExportEnabled = false; // Disable Export to CSV until patients are loaded

                if (SelectedYear == 0)
                {
                    Log.Warning("LoadAvailableMonthsAsync aborted: SelectedYear is 0.");
                    return;
                }

                var months = await _databaseService.GetAvailableMonthsAsync(SelectedYear);
                AvailableMonths.Clear();
                foreach (var month in months)
                {
                    AvailableMonths.Add(month);
                    Log.Verbose("Added month to AvailableMonths collection: {Month}", month);
                }

                Log.Information("LoadAvailableMonthsAsync completed. Months loaded: {MonthCount}", AvailableMonths.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while loading available months for year: {SelectedYear}", SelectedYear);
                MessageBox.Show("An error occurred while loading the available months. Please try again.", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task LoadPatientsAsync()
        {
            try
            {
                Log.Verbose("LoadPatientsAsync initiated.");

                if (SelectedYear == 0 || (SelectedMonth == 0 && !IsAllMonthsSelected))
                {
                    Log.Warning("LoadPatientsAsync aborted: SelectedYear is 0 or both SelectedMonth is 0 and IsAllMonthsSelected is false.");
                    return;
                }

                List<Patient> patients;
                if (IsAllMonthsSelected)
                {
                    Log.Verbose("Loading all patients for the entire year: {SelectedYear}", SelectedYear);
                    // Load all patients for the entire year
                    patients = await _databaseService.GetPatientsByYearAsync(SelectedYear);
                }
                else
                {
                    Log.Verbose("Loading patients for year: {SelectedYear}, month: {SelectedMonth}", SelectedYear, SelectedMonth);
                    // Load patients for the selected year and month
                    patients = await _databaseService.GetPatientsByYearAndMonthAsync(SelectedYear, SelectedMonth);
                }

                Patients.Clear();
                foreach (var patient in patients)
                {
                    Patients.Add(patient);
                    Log.Verbose("Added patient to Patients collection: {PatID}, {PatDicom}", patient.PatID, patient.PatDicom);
                }

                IsExportEnabled = Patients.Count > 0;
                Log.Information("LoadPatientsAsync completed. Patients loaded: {PatientCount}", Patients.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while loading patients.");
                MessageBox.Show("An error occurred while loading patients. Please try again.", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ExportToXlsx()
        {
            try
            {
                Log.Verbose("ExportToXlsx initiated.");

                if (Patients.Count == 0)
                {
                    Log.Warning("No patients to export.");
                    return;
                }

                string filePath = $"Patients_{SelectedYear}_{SelectedMonth}.xlsx";

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Patients");

                    // Adding headers
                    worksheet.Cells[1, 1].Value = "RecDateTime";
                    worksheet.Cells[1, 2].Value = "TimeLastUpdate";
                    worksheet.Cells[1, 3].Value = "PatDicom";
                    worksheet.Cells[1, 4].Value = "PatDOB";
                    worksheet.Cells[1, 5].Value = "PatID";
                    worksheet.Cells[1, 6].Value = "PatGender";
                    worksheet.Cells[1, 7].Value = "PatAge";
                    worksheet.Cells[1, 8].Value = "PatWeight";
                    worksheet.Cells[1, 9].Value = "PatComments";
                    worksheet.Row(1).Style.Font.Bold = true;
                    Log.Verbose("Headers added to the worksheet.");

                    // Adding patient data
                    for (int i = 0; i < Patients.Count; i++)
                    {
                        var patient = Patients[i];
                        worksheet.Cells[i + 2, 1].Value = patient.RecDateTime;
                        worksheet.Cells[i + 2, 2].Value = patient.TimeLastUpdate;
                        worksheet.Cells[i + 2, 3].Value = patient.PatDicom;
                        worksheet.Cells[i + 2, 4].Value = patient.PatDOB;
                        worksheet.Cells[i + 2, 5].Value = patient.PatID;
                        worksheet.Cells[i + 2, 6].Value = patient.PatGender;
                        worksheet.Cells[i + 2, 7].Value = patient.PatAge;
                        worksheet.Cells[i + 2, 8].Value = patient.PatWeight;
                        worksheet.Cells[i + 2, 9].Value = patient.PatComments;

                        // Format the date columns
                        worksheet.Cells[i + 2, 1].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                        worksheet.Cells[i + 2, 2].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";

                        Log.Verbose("Added patient data to row {RowNumber}: {PatID}, {PatDicom}", i + 2, patient.PatID, patient.PatDicom);
                    }

                    // Adjust columns to fit content
                    worksheet.Cells.AutoFitColumns();
                    Log.Verbose("AutoFitColumns applied.");

                    // Save the file
                    package.SaveAs(new FileInfo(filePath));
                    Log.Information("Patient data exported to XLSX successfully: {FilePath}", filePath);
                }

                // Open the folder where the file is saved
                OpenFolderAndSelectFile(filePath);

                // Notify the user
                MessageBox.Show($"Patient data exported to {filePath}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while exporting patients to XLSX.");
                MessageBox.Show("An error occurred while exporting the data to XLSX. Please try again.", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OpenFolderAndSelectFile(string filePath)
        {
            try
            {
                Log.Verbose("OpenFolderAndSelectFile initiated for file: {FilePath}", filePath);

                if (string.IsNullOrEmpty(filePath))
                {
                    Log.Warning("FilePath is null or empty.");
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true,
                    Verb = "open"
                });

                Log.Information("File opened successfully: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while opening the file: {FilePath}", filePath);
                MessageBox.Show("An error occurred while trying to open the file. Please try again.", "File Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ExportToCsv()
        {
            try
            {
                Log.Verbose("ExportToCsv initiated.");

                if (Patients.Count == 0)
                {
                    Log.Warning("No patients to export.");
                    return;
                }

                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("RecDateTime,TimeLastUpdate,PatDicom,PatDOB,PatID,PatGender,PatAge,PatWeight");

                foreach (var patient in Patients)
                {
                    csvBuilder.AppendLine($"{patient.RecDateTime},{patient.TimeLastUpdate},{patient.PatDicom},{patient.PatDOB},{patient.PatID},{patient.PatGender},{patient.PatAge},{patient.PatWeight}");
                    Log.Verbose("Added patient to CSV: {PatID}, {PatDicom}", patient.PatID, patient.PatDicom);
                }

                string filePath = $"Patients_{SelectedYear}_{SelectedMonth}.csv";
                File.WriteAllText(filePath, csvBuilder.ToString());

                Log.Information("Patient data exported to CSV successfully: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while exporting patients to CSV.");
                MessageBox.Show("An error occurred while exporting the data to CSV. Please try again.", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}