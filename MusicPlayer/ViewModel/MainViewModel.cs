using DevExpress.Mvvm;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using VKApplication.Model;
using VKApplication.App.Views;
using VKApplication.App.ViewModel;

namespace VKApplication.ViewModel
{
    public class MainViewModel : BaseVM
    {
        public MainViewModel()
        {
            OverlayService.GetInstance().Show = (str, vis) =>
            {
                OverlayService.GetInstance().Text = str;
                OverlayService.GetInstance().ProgressBarVisible = vis;
            };

            AudioService.GetMediaPlayer().MediaEnded += new EventHandler((s, e) =>
            {
                PlayerSwitch(PlayerCommandMethod.Ended);
            });


            Items = File.Exists("ItemsData.json")
                  ? JsonConvert.DeserializeObject<ObservableCollection<Item>>
                  (File.ReadAllText("ItemsData.json")) : new ObservableCollection<Item>();

            Items.CollectionChanged += (s, e) => 
                File.WriteAllText("ItemsData.json", JsonConvert.SerializeObject(Items));

            BindingOperations.EnableCollectionSynchronization(Items, new object());
            ItemsView = CollectionViewSource.GetDefaultView(Items);

            SelectedItem = Items.FirstOrDefault();
        }

        public string Title { get; set; } = Application.ResourceAssembly.FullName.Split(',')[0];
        public ObservableCollection<Item> Items { get; set; }
        public ICollectionView ItemsView { get; set; }
        public Page MainContent { get; set; }
        public Item SelectedItem { get; set; }
        public bool SelectedPlaylist { get; set; } = false;
        public bool IsRepeat { get; set; } = false;
        private string _SearchText { get; set; }
        public string SearchText
        {
            get => _SearchText;
            set
            {
                _SearchText = value;
                ItemsView.Filter = (obj) =>
                {
                    if (obj is Item item)
                    {
                        switch (SearchText.FirstOrDefault())
                        {
                           case '$':
                                if (DateTime.TryParse(SearchText.Remove(0, 1), out DateTime date))
                                    return (item.DateOfChange.Date == date.Date);
                                return false;

                            default: return item.Name.ToLower().Contains(SearchText.ToLower()) || item.Path.ToString().ToLower().Contains(SearchText.ToLower());
                        }
                    }

                    return false;
                };
                ItemsView.Refresh();

            }
        }
        
       
        public ICommand Sort
        {
            get
            {
                return new DelegateCommand<string>((p) =>
                {

                    if (ItemsView.SortDescriptions.Count > 0 && p != null)
                        ItemsView.SortDescriptions.Clear();
                    else if (ItemsView.SortDescriptions.Count > 0)
                    {
                        if (ItemsView.SortDescriptions[0].Direction == ListSortDirection.Ascending)
                            ItemsView.SortDescriptions.Insert(0, new SortDescription("Name", ListSortDirection.Descending));
                        else if (ItemsView.SortDescriptions[0].Direction == ListSortDirection.Descending)
                            ItemsView.SortDescriptions.Insert(0, new SortDescription("Name", ListSortDirection.Ascending));
                    }
                    else
                    {
                        ItemsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                    }
                });
            }
        }
        public ICommand SearchOfPlaylist
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (SelectedPlaylist == false)
                    {
                        SearchText = "";
                        ItemsView.Filter = (obj) =>
                        {
                            if (obj is Item item)
                            {
                                return (item.Playlist != null);
                            }
                            return false;
                        };
                        SelectedPlaylist = true;
                    }
                    else
                    {
                        ItemsView.Filter = null;
                        SelectedPlaylist = false;
                    }
                        ItemsView.Refresh();
                    
                }) ;
            }
        }
        public ICommand DeleteItem
        {
            get
            {
                return new DelegateCommand<Item>((item) =>
                {
                    Items.Remove(item);
                    SelectedItem = Items.FirstOrDefault();

                }, (item) => item != null);
            }
        }
        public ICommand AddItem
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    var opd = new OpenFileDialog();
                    opd.Title = "Выбор файлов";
                    opd.Multiselect = true;
                    //opd.Filter = "Audio (*.mp3,*.acc,*.wma,*.wav)|*.acc;*.mp3;*.wma;*.wav|" +
                    opd.Filter = "Audio (*.mp3)|*.mp3|" +
                                 "All Files (*.*)|*.*";

                    if (opd.ShowDialog() == true)
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            OverlayService.GetInstance().Show("Загрузка информации...", true);
                            int added = 0;
                            for (int i = 0; i < opd.FileNames.Length; i++)
                            {
                                OverlayService.GetInstance().Show($"Загрузка информации...{Environment.NewLine}{i}/{opd.FileNames.Length}", true);
                                var file = opd.FileNames[i];

                                Items.Add(new Item
                                {
                                    Name = Path.GetFileNameWithoutExtension(file),
                                    Type = Path.GetExtension(file),
                                    Size = new FileInfo(file).Length / 1024.0,
                                    Path = new Uri(Path.GetFullPath(file)),
                                    DateOfChange = new FileInfo(file).LastWriteTime,

                                });
                                added++;

                                Task.Delay(1).Wait();
                            }
                            SelectedItem = Items.FirstOrDefault();
                            OverlayService.GetInstance().Show($"Добавлено элементов: {added}", false);
                            Task.Delay(2000).Wait();
                            OverlayService.GetInstance().Close();
                        });
                    }

                });
            }
        }
        public ICommand FindItem
        {
            get
            {
                return new DelegateCommand(async () =>
                {
                    
                    var opd = new CommonOpenFileDialog();
                    opd.Title = "Выбор папки";
                    opd.Multiselect = false;
                    opd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    opd.IsFolderPicker = true;

                    if (opd.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        await Task.Factory.StartNew(() =>
                        {
                            OverlayService.GetInstance().Show("Загрузка информации...", true);

                            var files = GetFiles(opd.FileName, "*.mp3");
                            int added = 0;

                            foreach(string file in files)
                            {
                                try
                                {
                                    if (Path.GetExtension(file) == ".mp3")
                                    {
                                        OverlayService.GetInstance().Show($"Загрузка информации...\n{file}", true);
                                        Item newItem = new Item
                                        {
                                            Name = Path.GetFileNameWithoutExtension(file),
                                            Type = Path.GetExtension(file),
                                            Size = new FileInfo(file).Length / 1024.0,
                                            Path = new Uri(Path.GetFullPath(file)),
                                            DateOfChange = new FileInfo(file).LastWriteTime,
                                        };

                                        bool isExist = false;
                                        foreach (Item item in Items)
                                        {
                                            if (newItem.Name.Equals(item.Name))
                                                isExist = true;
                                        }

                                        if (!isExist)
                                        {
                                            Items.Add(newItem);
                                            added++;
                                        }

                                        Task.Delay(1).Wait();
                                    }

                                }
                                catch (Exception ex)
                                {
                                    OverlayService.GetInstance().Show($"Ошибка\n{ex.Message}",false);
                                }
                            }

                            //SelectedItem = Items.FirstOrDefault(s => s.Path == opd.FileNames.FirstOrDefault());
                            OverlayService.GetInstance().Show($"Добавлено элементов: {added}", false);
                            Task.Delay(2000).Wait();
                            OverlayService.GetInstance().Close();
                        });
                        

                    }

                });
            }
        }
        public ICommand EditItem
        {
            get
            {
                return new DelegateCommand<Item>((item) =>
                {
                    var w = new EditItemWindow();
                    var vm = new EditItemViewModel
                    {
                        ItemInfo = item,
                    };
                    w.DataContext = vm;
                    w.ShowDialog();

                }, (item) => item != null);
            }
        }
        public ICommand GoToUrl
        {
            get
            {
                return new DelegateCommand<Uri>((uri) =>
                {
                    if (uri.IsFile)
                    {
                        Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + uri.ToString()));
                    }
                    else
                    {
                        Process.Start(uri.ToString());
                    }


                });
            }
        }
        public ICommand DataClick
        {
            get
            {
                return new DelegateCommand<DateTime>((date) =>
                {
                    SearchText = "$" + date.Date.ToShortDateString();

                });
            }
        }
        public ICommand PlayPause
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PlayerSwitch(PlayerCommandMethod.PlayPause);
                }, () => !(SelectedItem is null) || !(AudioService.GetInstance().CurrentItem is null));
            }
        }
        public ICommand StopPlay
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PlayerSwitch(PlayerCommandMethod.Stop);
                }, () => AudioService.GetInstance().CurrentItem != null);
            }
        }
        public ICommand NextPlay
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PlayerSwitch(PlayerCommandMethod.Next);
                }, () => !ItemsView.IsEmpty);
            }
        }
        public ICommand PrevPlay
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PlayerSwitch(PlayerCommandMethod.Prev);
                }, () => !ItemsView.IsEmpty);
            }
        }
        public ICommand MoveForward
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PlayerSwitch(PlayerCommandMethod.MoveForward);
                }, () => AudioService.GetInstance().CurrentItem != null);
            }
        }
        public ICommand MoveBack
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PlayerSwitch(PlayerCommandMethod.MoveBack);
                }, () => AudioService.GetInstance().CurrentItem != null);
            }
        }
        public ICommand ChangeRepeat
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    IsRepeat = !IsRepeat;
                });
            }
        }
        public ICommand ChangeBookmark
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var a = AudioService.GetInstance().CurrentItem;
                    var i = Items.IndexOf(a);
                    if (a.Playlist == null)
                        a.Playlist = "1";
                    else 
                        a.Playlist = null;
                    Items.RemoveAt(i);
                    Items.Insert(i, a);
                }, ()=>AudioService.GetInstance().CurrentItem != null);
            }
        }


        /// <summary>
        /// Метод для переключения треков.
        /// </summary>
        /// <param name="method">
        /// Тип метода. Определяет как переключится трек.
        /// </param>
        private void PlayerSwitch(PlayerCommandMethod method)
        {
            var service = AudioService.GetInstance();

            switch (method)
            {
                case PlayerCommandMethod.PlayPause:
                    PlayPause();
                    break;
                case PlayerCommandMethod.Stop:
                    Stop();
                    break;
                case PlayerCommandMethod.Next:
                    Next();
                    break;
                case PlayerCommandMethod.Prev:
                    Prev();
                    break;
                case PlayerCommandMethod.Ended:
                    Ended();
                    break;
                case PlayerCommandMethod.MoveForward:
                    MoveForward();
                    break;
                case PlayerCommandMethod.MoveBack:
                    MoveBack();
                    break;
                default:
                    return;
            }

            void Next()
            {
                if (ItemsView.IsEmpty) return;
                ItemsView.MoveCurrentTo(service.CurrentItem);
                if (!ItemsView.MoveCurrentToNext())
                    ItemsView.MoveCurrentToFirst();
                service.StartPlay(ItemsView.CurrentItem as Item);
            }

            void Prev()
            {
                if (ItemsView.IsEmpty) return;
                ItemsView.MoveCurrentTo(service.CurrentItem);
                if (!ItemsView.MoveCurrentToPrevious())
                    ItemsView.MoveCurrentToLast();
                service.StartPlay(ItemsView.CurrentItem as Item);
            }

            void PlayPause()
            {
                if (service.CurrentItem != null)
                    service.PlayPause();
                else if (SelectedItem != null)
                    service.StartPlay(SelectedItem);
            }

            void Stop()
            {
                service.Stop();
            }
            
            void Ended()
            {
                if (IsRepeat)
                    service.Restart();
                else if (ItemsView.IsEmpty)
                    Stop();
                else
                    Next();
            }

            void MoveForward()
            {
                try
                {
                    service.MoveForward();
                }
                catch (Exception)
                {
                    Ended();
                }
            }

            void MoveBack()
            {
                service.MoveBack();
            }
        }

        private enum PlayerCommandMethod
        {
            PlayPause,
            Stop,
            Next,
            Prev,
            Ended,
            MoveForward,
            MoveBack
        }

        private System.Collections.Generic.List<string> GetFiles(string path, string pattern)
        {
            var files = new System.Collections.Generic.List<string>();

            try
            {
                files.AddRange(Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly));
                foreach (var directory in Directory.GetDirectories(path))
                    files.AddRange(GetFiles(directory, pattern));
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }

            return files;
        }

    }
}

