using OfficeOpenXml;
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

        public ICommand LoadPatientsCommand { get; }
        public ICommand ExportToCsvCommand { get; }

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Patients = new ObservableCollection<Patient>();
            AvailableYears = new ObservableCollection<int>();
            AvailableMonths = new ObservableCollection<int>();

            LoadPatientsCommand = new RelayCommand(async () => await LoadPatientsAsync(), () => IsLoadPatientsEnabled);
            ExportToCsvCommand = new RelayCommand(ExportToCsv, () => IsExportEnabled);

            LoadAvailableYearsAsync();

            ExportToXlsxCommand = new RelayCommand(ExportToXlsx, () => IsExportEnabled);

        }

        private async void LoadAvailableYearsAsync()
        {
            var years = await _databaseService.GetAvailableYearsAsync();
            AvailableYears.Clear();
            foreach (var year in years)
            {
                AvailableYears.Add(year);
            }
        }

        private async void LoadAvailableMonthsAsync()
        {
            IsLoadPatientsEnabled = false; // Disable Load Patients button until both Year and Month are selected
            IsExportEnabled = false; // Disable Export to CSV until patients are loaded

            if (SelectedYear == 0)
                return;

            var months = await _databaseService.GetAvailableMonthsAsync(SelectedYear);
            AvailableMonths.Clear();
            foreach (var month in months)
            {
                AvailableMonths.Add(month);
            }
        }

        private async Task LoadPatientsAsync()
        {
            if (SelectedYear == 0 || (SelectedMonth == 0 && !IsAllMonthsSelected))
                return;

            List<Patient> patients;
            if (IsAllMonthsSelected)
            {
                // Load all patients for the entire year
                patients = await _databaseService.GetPatientsByYearAsync(SelectedYear);
            }
            else
            {
                // Load patients for the selected year and month
                patients = await _databaseService.GetPatientsByYearAndMonthAsync(SelectedYear, SelectedMonth);
            }

            Patients.Clear();
            foreach (var patient in patients)
            {
                Patients.Add(patient);
            }

            IsExportEnabled = Patients.Count > 0;
        }

        private void ExportToXlsx()
        {
            if (Patients.Count == 0)
                return;

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
                }

                // Adjust columns to fit content
                worksheet.Cells.AutoFitColumns();

                // Save the file
                package.SaveAs(new FileInfo(filePath));
            }

            // Open the folder where the file is saved
            OpenFolderAndSelectFile(filePath);

            // Notify the user
            MessageBox.Show($"Patient data exported to {filePath}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenFolderAndSelectFile(string filePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void ExportToCsv()
        {
            if (Patients.Count == 0)
                return;

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("RecDateTime,TimeLastUpdate,PatDicom,PatDOB,PatID,PatGender,PatAge,PatWeight");

            foreach (var patient in Patients)
            {
                csvBuilder.AppendLine($"{patient.RecDateTime},{patient.TimeLastUpdate},{patient.PatDicom},{patient.PatDOB},{patient.PatID},{patient.PatGender},{patient.PatAge},{patient.PatWeight}");
            }

            string filePath = $"Patients_{SelectedYear}_{SelectedMonth}.csv";
            File.WriteAllText(filePath, csvBuilder.ToString());

            MessageBox.Show($"Patient data exported to {filePath}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}