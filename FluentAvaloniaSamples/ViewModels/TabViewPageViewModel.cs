using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using FluentAvaloniaSamples.Utilities;

namespace FluentAvaloniaSamples.ViewModels
{
    public class TabViewPageViewModel : ViewModelBase
    {
        public TabViewPageViewModel()
        {
            Documents = new ObservableCollection<DocumentItem>();
            for (int i = 0; i < 3; i++)
            {
                Documents.Add(AddDocument(i));
            }

            KeyBindingDocuments = new ObservableCollection<DocumentItem>();
            for (int i = 0; i < 3; i++)
            {
                KeyBindingDocuments.Add(AddDocument(i));
            }

            AddDocumentCommand = new FACommand(AddDocumentExecute);
            KeyBindingCommand = new FACommand(KeyBindingInvoked);
        }

        public ObservableCollection<DocumentItem> Documents { get; } 

        public ObservableCollection<DocumentItem> KeyBindingDocuments { get; }

        public DocumentItem KeyBindingSelectedDocument
        {
            get => _keybindingSelectedDocument;
            set => RaiseAndSetIfChanged(ref _keybindingSelectedDocument, value);
        }

        public FACommand AddDocumentCommand { get; }

        public FACommand KeyBindingCommand { get; }

        public string KeyBindingText { get; set; }

        private void AddDocumentExecute(object obj)
        {
            if (obj.Equals("Keyboard"))
            {
                KeyBindingDocuments.Add(AddDocument(KeyBindingDocuments.Count));
            }
            else if (obj.Equals("Normal"))
            {
                Documents.Add(AddDocument(Documents.Count));
            }
        }

        private void KeyBindingInvoked(object obj)
        {
            var str = obj.ToString();
            if (int.TryParse(str, out var result))
            {
                if (KeyBindingDocuments.Count == 0)
                    return;

                if (result == 9)
                {
                    KeyBindingSelectedDocument = KeyBindingDocuments[^1];
                }
                else if (KeyBindingDocuments.Count >= result)
                {
                    KeyBindingSelectedDocument = KeyBindingDocuments[result-1];
                }
            }
            else if (str.Equals("Add"))
            {
                KeyBindingDocuments.Add(AddDocument(KeyBindingDocuments.Count));
            }
            else if (str.Equals("Close"))
            {
                KeyBindingDocuments.Remove(KeyBindingSelectedDocument);
            }
        }

        private DocumentItem AddDocument(int index)
        {
            var tab = new DocumentItem
            {
                Header = $"My document {index}"
            };

            switch (index % 3)
            {
                case 0:
                    tab.IconSource = new SymbolIconSource { Symbol = Symbol.Document };
                    tab.Content = "This is a sample document. Switch tabs to view more.";
                    break;

                case 1:
                    tab.IconSource = new SymbolIconSource { Symbol = Symbol.Star };
                    tab.Content = "This is another sample document. Switch tabs to view more.";
                    break;

                case 2:
                    tab.IconSource = new SymbolIconSource { Symbol = Symbol.Open };
                    tab.Content = "This is yet another sample document. Switch tabs to view more.";
                    break;
            }

            return tab;
        }

        private DocumentItem _keybindingSelectedDocument;
    }

    public class DocumentItem
    {
        public string Header { get; set; }

        public IconSource IconSource { get; set; }

        public string Content { get; set; }
    }
}
