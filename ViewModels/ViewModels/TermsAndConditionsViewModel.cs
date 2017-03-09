using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using BaseObjects;
using BaseObjects.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportBetting.WPF.Prism.Shared;
using DocumentFormat.OpenXml.Packaging;
using Bold = DocumentFormat.OpenXml.Wordprocessing.Bold;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    [ServiceAspect]
    public class TermsAndConditionsViewModel : BaseViewModel
    {
        private readonly ScrollViewerModule _ScrollViewerModule;
        private List<TermsAndConditionsCategory> _categories = new List<TermsAndConditionsCategory>();

        #region Constructor and destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TermsAndConditionsViewModel"/> class.
        /// </summary>
        public TermsAndConditionsViewModel()
        {
            _ScrollViewerModule = new ScrollViewerModule(Dispatcher);
            CloseCommand = new Command(CloseAndCheckTermsVersion);
            ScrollDownStart = new Command(OnScrollDownStartExecute);
            ScrollDownStop = new Command(OnScrollDownStopExecute);
            ScrollUpStart = new Command(OnScrollUpStartExecute);
            ScrollUpStop = new Command(OnScrollUpStopExecute);
            MenuClick = new Command<TCMenuButton>(OnMenuClick);
            SwitchCategoryCommand = new Command<string>(OnSwitchCategoryCommand);
            ButtonGeneralSelected = true;
            GetRulesFromFiles();

            GetLeftButtonsForCategories();

            Mediator.Register<bool>(this, CloseCurrentWindow, MsgTag.CloseCurrentWindow);
            Mediator.Register<bool>(this, GoToVHCPart, MsgTag.OpenVHCtAC);
            Mediator.Register<bool>(this, GoToResponsible, MsgTag.ShowResponibleGamng);
        }

        private void GetLeftButtonsForCategories()
        {
            foreach (var category in _categories)
                foreach (var dHeading in category.Headings)
                    foreach (var vals in dHeading.Value)
                    {
                        TCMenuButton buttonX = new TCMenuButton();

                        if (vals.Key == "")
                        {
                            category.Buttons.Add(new TCMenuButton()
                                {
                                    Text = dHeading.Key,
                                    Children = new ObservableCollection<TCMenuButton>(),
                                    Selected = false
                                });
                        }
                        else
                        {
                            if (SkipVflOrVhc(vals.Key))
                                continue;
                            category.Buttons[category.Buttons.Count - 1].Children.Add(new TCMenuButton() { Text = vals.Key, Selected = false });
                        }
                    }
        }

        private bool SkipVflOrVhc(string key)
        {
            return (key.EndsWith("#VFL#") && !StationRepository.AllowVfl) ||
                   (key.EndsWith("#VHC#") && !StationRepository.AllowVhc);
        }

        public override void OnNavigationCompleted()
        {
            var button = GetSelectedCategory().Buttons.First(b => b.Enabled);
            button.Selected = true;
            OnMenuClick(button);

            //ButtonGeneralEnabled = false;

            OnPropertyChanged("ButtonGeneralEnabled");
            OnPropertyChanged("ButtonPrematchEnabled");
            OnPropertyChanged("ButtonLiveEnabled");
            OnPropertyChanged("ButtonVirtualEnabled");

            OnPropertyChanged("ButtonGeneralSelected");
            OnPropertyChanged("ButtonPrematchSelected");
            OnPropertyChanged("ButtonLiveSelected");
            OnPropertyChanged("ButtonVirtualSelected");

            base.OnNavigationCompleted();
        }

        #endregion

        #region Properties

        public bool ButtonGeneralEnabled { get; set; }
        public bool ButtonPrematchEnabled { get; set; }
        public bool ButtonLiveEnabled { get; set; }
        public bool ButtonVirtualEnabled { get; set; }

        public bool ButtonGeneralSelected { get; set; }
        public bool ButtonPrematchSelected { get; set; }
        public bool ButtonLiveSelected { get; set; }
        public bool ButtonVirtualSelected { get; set; }

        public ObservableCollection<TCMenuButton> LeftButtons
        {
            get
            {
                var cat = GetSelectedCategory();
                return cat.Buttons;
            }
        }

        private FlowDocument _currentBlock = new FlowDocument();
        public FlowDocument CurrentBlock
        {
            get { return _currentBlock; }
            set
            {
                _currentBlock = value;
                OnPropertyChanged("CurrentBlock");
            }
        }

        /// <summary>
        /// Gets or sets the Text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Register the Text property so it is known in the class.
        /// </summary>

        #endregion

        #region Commands

        /// <summary>
        /// Gets the ScrollDownStart command.
        /// </summary>
        public Command ScrollDownStart { get; private set; }
        /// <summary>
        /// Gets the ScrollDownStop command.
        /// </summary>
        public Command ScrollDownStop { get; private set; }
        /// <summary>
        /// Gets the ScrollUpStart command.
        /// </summary>
        public Command ScrollUpStart { get; private set; }
        /// <summary>
        /// Gets the ScrollUpStop command.
        /// </summary>
        public Command ScrollUpStop { get; private set; }

        public Command CloseCommand { get; private set; }

        public Command<TCMenuButton> MenuClick { get; private set; }

        public Command<string> SwitchCategoryCommand { get; private set; }

        #endregion

        #region Scroller Procedures
        public ScrollViewer GetScrollviewer()
        {
            var mainWindow = GetActiveWindow();
            if (mainWindow == null) { return null; }

            FlowDocumentScrollViewer dObj = AppVisualTree.FindChild<System.Windows.Controls.FlowDocumentScrollViewer>(mainWindow, "flowScroller");
            if (dObj == null) return null;

            DependencyObject firstChild = VisualTreeHelper.GetChild(dObj, 0);
            if (firstChild == null) return null;

            Decorator border = VisualTreeHelper.GetChild(firstChild, 0) as Decorator;

            return border.Child as ScrollViewer;

        }

        /// <summary>
        /// Method to invoke when the ScrollDownStart command is executed.
        /// </summary>
        private void OnScrollDownStartExecute()
        {
            this._ScrollViewerModule.OnScrollDownStartExecute(this.GetScrollviewer(), true);
        }
        /// <summary>
        /// Method to invoke when the ScrollDownStop command is executed.
        /// </summary>
        private void OnScrollDownStopExecute()
        {
            this._ScrollViewerModule.OnScrollDownStopExecute();
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStart command is executed.
        /// </summary>
        private void OnScrollUpStartExecute()
        {
            this._ScrollViewerModule.OnScrollUpStartExecute(this.GetScrollviewer(), true);
        }
        /// <summary>
        /// Method to invoke when the ScrollUpStop command is executed.
        /// </summary>
        private void OnScrollUpStopExecute()
        {
            this._ScrollViewerModule.OnScrollUpStopExecute();
        }
        #endregion

        #region Methods

        private void GoToVHCPart(bool res)
        {
            ButtonGeneralSelected = false;
            ButtonPrematchSelected = false;
            ButtonLiveSelected = false;
            ButtonVirtualSelected = true;

            GetSelectedCategory().Buttons[0].Selected = true;
            List<TCMenuButton> menu = GetSelectedCategory().Buttons.ToList();

            if (menu.Count > 0)
            {
                foreach (TCMenuButton button in menu[0].Children)
                {
                    if (button.Text.EndsWith("#VHC#"))
                    {
                        OnMenuClick(button);
                        break;
                    }
                }
            }

            OnPropertyChanged("ButtonGeneralSelected");
            OnPropertyChanged("ButtonPrematchSelected");
            OnPropertyChanged("ButtonLiveSelected");
            OnPropertyChanged("ButtonVirtualSelected");
        }

        private void GoToResponsible(bool res)
        {
            ButtonGeneralSelected = true;
            ButtonPrematchSelected = false;
            ButtonLiveSelected = false;
            ButtonVirtualSelected = false;

            GetSelectedCategory().Buttons[0].Selected = true;
            List<TCMenuButton> menu = GetSelectedCategory().Buttons.ToList();

            if (menu.Count > 0)
            {
                foreach (TCMenuButton button in menu)
                {
                    if (button.Text.EndsWith("#RG#"))
                    {
                        OnMenuClick(button);
                        break;
                    }
                }
            }

            OnPropertyChanged("ButtonGeneralSelected");
            OnPropertyChanged("ButtonPrematchSelected");
            OnPropertyChanged("ButtonLiveSelected");
            OnPropertyChanged("ButtonVirtualSelected");
        }

        public void OnMenuClick(TCMenuButton tcMenuButton)
        {
            foreach (var menuButton in GetSelectedCategory().Buttons)
            {
                foreach (var button in menuButton.Children)
                {
                    button.Selected = button.Text == tcMenuButton.Text;
                }
            }

            if (tcMenuButton.Children != null)
            {
                // only pwhen top-menu button clicked    
                foreach (var menuButton in GetSelectedCategory().Buttons)
                {
                    menuButton.Selected = false;
                }
                var buttonX = GetSelectedCategory().Buttons.FirstOrDefault(x => x.Text == tcMenuButton.Text) as TCMenuButton;
                if (buttonX != null)
                {
                    buttonX.Selected = true;
                }
            }

            GetCurrentBlock(GetSelectedCategory(), tcMenuButton.Text, tcMenuButton.Children == null);
            OnPropertyChanged("LeftButtons");
        }

        private TermsAndConditionsCategory GetSelectedCategory()
        {
            if (ButtonGeneralSelected)
                return _categories.SingleOrDefault(c => c.Category.Equals("General"));
            if (ButtonPrematchSelected)
                return _categories.SingleOrDefault(c => c.Category.Equals("Prematch"));
            if (ButtonLiveSelected)
                return _categories.SingleOrDefault(c => c.Category.Equals("Live"));
            if (ButtonVirtualSelected)
                return _categories.SingleOrDefault(c => c.Category.Equals("Virtual"));
            return null;
        }

        public void CloseAndCheckTermsVersion()
        {
            Close();
            Mediator.SendMessage<bool>(true, MsgTag.AcceptNewTermsVersion);
        }

        public void CloseCurrentWindow(bool state)
        {
            Close();
            Mediator.SendMessage<bool>(true, MsgTag.AcceptNewTermsVersion);
        }

        private string GetFileInFolderByLanguage(string folder)
        {
            string[] filenames = { TranslationProvider.CurrentLanguage, "Default" };
            foreach (var filename in filenames)
            {
                string fileX = Environment.CurrentDirectory.ToString() + "\\" + StationRepository.LayoutName + @"\TermsAndConditions\" + folder + @"\" + filename + @".docx";
                if (File.Exists(fileX))
                {
                    EnableButtonByFolder(folder);
                    return fileX;
                }
                fileX = Environment.CurrentDirectory.ToString() + "\\" + BaseObjects.MyRegionManager.DefaultLayout + @"\TermsAndConditions\" + folder + @"\" + filename + @".docx";
                if (File.Exists(fileX))
                {
                    EnableButtonByFolder(folder);
                    return fileX;
                }
            }
            return null;
        }

        private List<String> GetFolders()
        {
            var folders = new List<string> { "General" };
            if (StationRepository.IsPrematchEnabled)
                folders.Add("Prematch");
            if (StationRepository.IsLiveMatchEnabled)
                folders.Add("Live");
            if (StationRepository.AllowVfl || StationRepository.AllowVhc)
                folders.Add("Virtual");
            return folders;
        }

        private void EnableButtonByFolder(String folder)
        {
            switch (folder)
            {
                case "General":
                    ButtonGeneralEnabled = true;
                    break;
                case "Prematch":
                    ButtonPrematchEnabled = true;
                    break;
                case "Live":
                    ButtonLiveEnabled = true;
                    break;
                case "Virtual":
                    ButtonVirtualEnabled = true;
                    break;
            }
        }

        private void GetRulesFromFiles()
        {
            foreach (var folder in GetFolders())
            {
                string fileX = GetFileInFolderByLanguage(folder);
                if (fileX == null) continue;
                var category = new TermsAndConditionsCategory(folder);

                using (WordprocessingDocument xmlX = WordprocessingDocument.Open(fileX, false))
                {
                    int iX = 0;

                    string sCurH1 = "";
                    string sCurH2 = "";
                    List<string> lstTexts = new List<string>();
                    int? iParCount = xmlX.MainDocumentPart.Document.Body.Descendants<Paragraph>().Count();

                    foreach (var descendant in xmlX.MainDocumentPart.Document.Body.Descendants<Paragraph>())
                    {
                        iX++;
                        if ((descendant.ParagraphProperties != null && descendant.ParagraphProperties.ParagraphStyleId != null) || iX == iParCount)
                        {
                            string sHeading;
                            try
                            {
                                sHeading = descendant.ParagraphProperties.ParagraphStyleId.Val.Value;
                            }
                            catch (Exception)
                            {
                                sHeading = "";
                            }

                            if (sHeading == "Heading1" || iX == iParCount)
                            {
                                if (lstTexts.Count > 0)
                                {

                                    category.Texts.Add(sCurH2, lstTexts);
                                    lstTexts = new List<string>();

                                    category.Headings.Add(sCurH1, category.Texts);
                                    category.Texts = new Dictionary<string, List<string>>();

                                }
                                sCurH1 = descendant.InnerText;
                                sCurH2 = "";
                            }
                            else if (sHeading == "Heading2")
                            {
                                var text = descendant.InnerText;
                                //if (!SkipVflOrVhc(text))
                                //    text = text.Replace("#VFL#", "").Replace("#VHC#", ""); //have to save those somehow...
                                if (lstTexts.Count > 0)
                                {
                                    category.Texts.Add(sCurH2, lstTexts);
                                    lstTexts = new List<string>();
                                }
                                sCurH2 = text;
                            }
                            else if (sHeading != "Title")
                            {

                                if (sHeading == "Heading3")
                                {
                                    lstTexts.Add("H3|" + descendant.InnerText);
                                }
                                else if (sHeading == "Heading4")
                                {
                                    lstTexts.Add("H4|" + descendant.InnerText);
                                }
                                else if (sHeading == "ListParagraph")
                                {
                                    lstTexts.Add("LP|" + descendant.InnerText);
                                }
                                else
                                {
                                    lstTexts.Add(descendant.InnerText);
                                }

                                //Debug.WriteLine(descendant.InnerText);
                            }
                            //Debug.WriteLine(sHeading + " - " + descendant.InnerText);
                        }
                        else
                        {
                            if (descendant.ParagraphProperties != null
                                && descendant.ParagraphProperties.ParagraphMarkRunProperties != null
                                && descendant.ParagraphProperties.ParagraphMarkRunProperties.Any(p => p is Bold))
                                lstTexts.Add("B|" + descendant.InnerText);
                            else
                                //usual paragraph
                                lstTexts.Add("|" + descendant.InnerText);
                            //Debug.WriteLine(descendant.InnerText);
                        }
                    }
                }
                _categories.Add(category);
            }
        }

        private void GetCurrentBlock(TermsAndConditionsCategory category, string sKey, bool secondLevelMenu = false)
        {
            ScrollViewer sv = GetScrollviewer();
            sv.ScrollToVerticalOffset(0);
            FlowDocument fldX = new FlowDocument();
            foreach (var dHeading in category.Headings)
            {
                if (!secondLevelMenu && dHeading.Key == sKey)
                {
                    System.Windows.Documents.Paragraph pX = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(dHeading.Key.Replace("#VHC#", "").Replace("#VFL#", "").Replace("#RG#", "")));
                    pX.FontFamily = new System.Windows.Media.FontFamily("Play");
                    pX.FontSize = 28;
                    pX.FontWeight = FontWeights.Bold;
                    pX.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D6AC0A"));
                    fldX.Blocks.Add(pX);
                }
                else
                {
                    if (!secondLevelMenu) continue;
                }

                var sectionX = category.Headings[dHeading.Key];

                foreach (var sX in sectionX)
                {
                    if ((!secondLevelMenu && sX.Key != "") || (secondLevelMenu && sX.Key != sKey))
                    {
                        continue;
                    }

                    if (secondLevelMenu)
                    {
                        System.Windows.Documents.Paragraph pX = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(sKey.Replace("#VHC#", "").Replace("#VFL#", "").Replace("#RG#", "")));
                        pX.FontFamily = new System.Windows.Media.FontFamily("Play");
                        pX.FontSize = 24;
                        pX.FontWeight = FontWeights.Bold;
                        pX.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D6AC0A"));
                        fldX.Blocks.Add(pX);
                    }

                    foreach (var tX in sX.Value)
                    {
                        string[] arrVals = tX.Split('|');

                        if (arrVals[0] != "LP")
                        {
                            System.Windows.Documents.Paragraph ptX = new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(arrVals[1]));
                            ptX.FontFamily = new System.Windows.Media.FontFamily("Play");
                            switch (arrVals[0])
                            {
                                default:
                                    ptX.FontSize = 20;
                                    ptX.FontWeight = FontWeights.Normal;
                                    ptX.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E4E4E4"));
                                    break;
                                case "H3":
                                    ptX.FontSize = 20;
                                    ptX.FontWeight = FontWeights.Bold;
                                    ptX.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D6AC0A"));
                                    break;
                                case "H4":
                                    ptX.FontSize = 20;
                                    ptX.FontWeight = FontWeights.Bold;
                                    ptX.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E4E4E4"));
                                    break;
                                case "B":
                                    ptX.FontSize = 20;
                                    ptX.FontWeight = FontWeights.Bold;
                                    ptX.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E4E4E4"));
                                    break;
                            }

                            fldX.Blocks.Add(ptX);
                        }
                        else
                        {
                            List l = new List();
                            l.MarkerStyle = TextMarkerStyle.Disc;
                            l.FontFamily = new System.Windows.Media.FontFamily("Play");
                            l.FontSize = 20;
                            l.FontWeight = FontWeights.Normal;
                            l.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#E4E4E4"));

                            l.ListItems.Add(new System.Windows.Documents.ListItem(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(arrVals[1]))));
                            fldX.Blocks.Add(l);
                        }

                    }
                }
            }
            CurrentBlock = fldX;
        }

        private void OnSwitchCategoryCommand(string category)
        {
            ButtonGeneralSelected = false;
            ButtonPrematchSelected = false;
            ButtonLiveSelected = false;
            ButtonVirtualSelected = false;
            switch (category)
            {
                case "General":
                    ButtonGeneralSelected = true;
                    break;
                case "Prematch":
                    ButtonPrematchSelected = true;
                    break;
                case "Live":
                    ButtonLiveSelected = true;
                    break;
                case "Virtual":
                    ButtonVirtualSelected = true;
                    break;
            }

            GetSelectedCategory().Buttons[0].Selected = true;
            OnMenuClick(GetSelectedCategory().Buttons[0]);

            OnPropertyChanged("ButtonGeneralSelected");
            OnPropertyChanged("ButtonPrematchSelected");
            OnPropertyChanged("ButtonLiveSelected");
            OnPropertyChanged("ButtonVirtualSelected");
        }

        #endregion
    }
}
