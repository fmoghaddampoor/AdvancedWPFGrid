using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using AdvancedWPFGrid.Columns;

namespace AdvancedWPFGrid.Demo;

public partial class MainWindow : Window
{
    private ObservableCollection<SampleItem> _items = new();
    private bool _isGrouped = false;

    private static readonly string[] Categories = { "Electronics", "Clothing", "Home & Garden", "Sports", "Books", "Toys", "Food", "Health" };
    private static readonly string[] Statuses = { "Active", "Pending", "Completed", "Cancelled", "On Hold" };
    private static readonly string[] Priorities = { "Low", "Medium", "High", "Critical" };
    private static readonly string[] FirstNames = { "John", "Jane", "Michael", "Sarah", "David", "Emma", "Robert", "Lisa", "William", "Emily" };
    private static readonly string[] LastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Martinez", "Anderson" };

    public MainWindow()
    {
        InitializeComponent();
        SetupIconColumns();
    }

    private void SetupIconColumns()
    {
        // Set icons for the icon button columns
        var columns = MainGrid.Columns;
        
        // Find and configure icon columns
        foreach (var column in columns)
        {
            if (column is IconButtonColumn iconCol)
            {
                if (iconCol.ToolTipText == "Edit Item")
                {
                    iconCol.IconData = IconButtonColumn.EditIcon;
                    iconCol.ButtonClick += OnEditClick;
                }
                else if (iconCol.ToolTipText == "Delete Item")
                {
                    iconCol.IconData = IconButtonColumn.DeleteIcon;
                    iconCol.ButtonClick += OnDeleteClick;
                }
            }
            else if (column is ButtonColumn buttonCol)
            {
                buttonCol.ButtonClick += OnViewClick;
            }
            else if (column is ComboBoxColumn comboCol)
            {
                // Set ItemsSource for combo columns
                if (comboCol.Header == "Category")
                {
                    comboCol.ItemsSource = Categories;
                }
                else if (comboCol.Header == "Status")
                {
                    comboCol.ItemsSource = Statuses;
                }
            }
        }
    }

    private void LoadDataButton_Click(object sender, RoutedEventArgs e)
    {
        LoadData(100000);
    }

    private void LoadMillionButton_Click(object sender, RoutedEventArgs e)
    {
        LoadData(1000000);
    }

    private void LoadData(int count)
    {
        var sw = Stopwatch.StartNew();
        StatusText.Text = $"Loading {count:N0} rows...";

        _items = new ObservableCollection<SampleItem>();
        var random = new Random(42);

        for (int i = 0; i < count; i++)
        {
            _items.Add(new SampleItem
            {
                Id = i + 1,
                Name = $"{FirstNames[random.Next(FirstNames.Length)]} {LastNames[random.Next(LastNames.Length)]}",
                Description = $"Sample item description for row {i + 1}",
                Category = Categories[random.Next(Categories.Length)],
                Status = Statuses[random.Next(Statuses.Length)],
                Priority = Priorities[random.Next(Priorities.Length)],
                Value = Math.Round(random.NextDouble() * 10000, 2),
                Quantity = random.Next(1, 500),
                CreatedDate = DateTime.Now.AddDays(-random.Next(365)).ToString("yyyy-MM-dd"),
                IsActive = random.Next(2) == 1,
                IsSelected = false
            });
        }

        MainGrid.ItemsSource = _items;

        sw.Stop();
        StatusText.Text = $"Loaded {count:N0} rows successfully";
        RowCountText.Text = $"{count:N0} rows";
        LoadTimeText.Text = $"Load time: {sw.ElapsedMilliseconds}ms";
    }

    private void GroupButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isGrouped)
        {
            MainGrid.GroupManager.ClearGrouping();
            _isGrouped = false;
            StatusText.Text = "Grouping cleared";
        }
        else
        {
            // Apply 3-level grouping: Category -> Status -> Priority
            MainGrid.GroupDescriptions?.Clear();
            MainGrid.GroupDescriptions?.Add(new PropertyGroupDescription("Category"));
            MainGrid.GroupDescriptions?.Add(new PropertyGroupDescription("Status"));
            MainGrid.GroupDescriptions?.Add(new PropertyGroupDescription("Priority"));
            MainGrid.GroupManager.ApplyGrouping();
            _isGrouped = true;
            StatusText.Text = "Grouping applied: Category → Status → Priority";
        }
    }

    private void ExpandAllButton_Click(object sender, RoutedEventArgs e)
    {
        MainGrid.GroupManager.ExpandAllGroups();
        StatusText.Text = "All groups expanded";
    }

    private void CollapseAllButton_Click(object sender, RoutedEventArgs e)
    {
        MainGrid.GroupManager.CollapseAllGroups();
        StatusText.Text = "All groups collapsed";
    }

    private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
    {
        MainGrid.FilterManager.ClearAllFilters();
        StatusText.Text = "All filters cleared";
    }

    private void ClearSortButton_Click(object sender, RoutedEventArgs e)
    {
        MainGrid.SortManager.ClearAllSorts();
        StatusText.Text = "All sorts cleared";
    }

    private void OnViewClick(object? sender, ButtonClickEventArgs e)
    {
        if (e.DataItem is SampleItem item)
        {
            MessageBox.Show($"Viewing item: {item.Name}\nID: {item.Id}\nCategory: {item.Category}", 
                "View Item", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OnEditClick(object? sender, ButtonClickEventArgs e)
    {
        if (e.DataItem is SampleItem item)
        {
            MessageBox.Show($"Editing item: {item.Name}", "Edit Item", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OnDeleteClick(object? sender, ButtonClickEventArgs e)
    {
        if (e.DataItem is SampleItem item)
        {
            var result = MessageBox.Show($"Delete item: {item.Name}?", "Confirm Delete", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                _items.Remove(item);
                RowCountText.Text = $"{_items.Count:N0} rows";
                StatusText.Text = $"Deleted item: {item.Name}";
            }
        }
    }
}

/// <summary>
/// Sample data item for demonstration.
/// </summary>
public class SampleItem : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _category = string.Empty;
    private string _status = string.Empty;
    private string _priority = string.Empty;
    private double _value;
    private int _quantity;
    private string _createdDate = string.Empty;
    private bool _isActive;
    private bool _isSelected;

    public int Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public string Category
    {
        get => _category;
        set => SetField(ref _category, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public string Priority
    {
        get => _priority;
        set => SetField(ref _priority, value);
    }

    public double Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    public int Quantity
    {
        get => _quantity;
        set => SetField(ref _quantity, value);
    }

    public string CreatedDate
    {
        get => _createdDate;
        set => SetField(ref _createdDate, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetField(ref _isActive, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
