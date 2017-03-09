using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using BaseObjects;
using BaseObjects.ViewModels;
using SharedInterfaces;
using SportBetting.WPF.Prism.Modules.Keyboard.KeyAssignmentSets;
using SportBetting.WPF.Prism.Shared;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TranslationByMarkupExtension;
using IocContainer;
using Ninject;
using System.Collections.Generic;

namespace SportBetting.WPF.Prism.Modules.Keyboard.ViewModels
{
    /// <summary>
    /// The KeyboardViewModel is (duh) the view-model for the Virtual Keyboard Window.
    /// It's DataContext gets set to this.
    /// </summary>
    public class KeyboardViewModel : BaseViewModel, INotifyPropertyChanged    {

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public KeyboardViewModel()
        {
            Mediator.Register<string>(this, OnShowKeyboard, MsgTag.ShowKeyboard);
            Mediator.Register<string>(this, OnHideKeyboard, MsgTag.HideKeyboard);

            _VK_SPACE.IsLetter = false;
            _VK_SPACE.UnshiftedCodePoint = ' ';
            _Tab.IsLetter = false;
            _Tab.UnshiftedCodePoint = '\t';
            _vms = new ObservableCollection<KeyViewModel>
			       	{
			       		_VK_OEM_3,
			       		_VK_1,
			       		_VK_2,
			       		_VK_3,
			       		_VK_4,
			       		_VK_5,
			       		_VK_6,
			       		_VK_7,
			       		_VK_8,
			       		_VK_9,
			       		_VK_0,
			       		_VK_OEM_MINUS,
			       		_VK_OEM_PLUS,
			       		_VK_Q,
			       		_VK_W,
			       		_VK_E,
			       		_VK_R,
			       		_VK_T,
			       		_VK_Y,
			       		_VK_U,
			       		_VK_I,
			       		_VK_O,
			       		_VK_P,
			       		_VK_OEM_4,
			       		_VK_OEM_6,
			       		_VK_OEM_5,
			       		_VK_A,
			       		_VK_S,
			       		_VK_D,
			       		_VK_F,
			       		_VK_G,
			       		_VK_H,
			       		_VK_J,
			       		_VK_K,
			       		_VK_L,
			       		_VK_OEM_1,
			       		_VK_OEM_7,
			       		_VK_Z,
			       		_VK_X,
			       		_VK_C,
			       		_VK_V,
			       		_VK_B,
			       		_VK_N,
			       		_VK_M,
			       		_VK_OEM_COMMA,
			       		_VK_OEM_PERIOD,
			       		_VK_OEM_2,
			       		_VK_AT
			       	};
            foreach (var keyViewModel in _vms)
            {
                keyViewModel.s_domain = this;
            }

            //public enum WhichKeyboardLayout { Unknown, English, Arabic, French, German, Italian, Mandarin, Russian, Turkish, Serbian };
            WhichKeyboardLayout lastKeyboardLayout;
            string currentLanguage = TranslationProvider.CurrentLanguage;
            if(currentLanguage.ToLowerInvariant() == "en")
                lastKeyboardLayout = WhichKeyboardLayout.English;
            else if(currentLanguage.ToLowerInvariant() == "de")
                lastKeyboardLayout = WhichKeyboardLayout.German;
            else if (currentLanguage.ToLowerInvariant() == "it")
                lastKeyboardLayout = WhichKeyboardLayout.Italian;
            else if (currentLanguage.ToLowerInvariant() == "srl")
                lastKeyboardLayout = WhichKeyboardLayout.Serbian;
            else if (currentLanguage.ToLowerInvariant() == "ru")
                lastKeyboardLayout = WhichKeyboardLayout.Russian;
            else if (currentLanguage.ToLowerInvariant() == "tr")
                lastKeyboardLayout = WhichKeyboardLayout.Turkish;
            else if (currentLanguage.ToLowerInvariant() == "fr")
                lastKeyboardLayout = WhichKeyboardLayout.French;
            else if (currentLanguage.ToLowerInvariant() == "zh")
                lastKeyboardLayout = WhichKeyboardLayout.Mandarin;
            else 
                lastKeyboardLayout = WhichKeyboardLayout.English;

            SelectedKeyboardLayout = lastKeyboardLayout;
            SetKeyAssignmentsTo(lastKeyboardLayout);
            KeyPressCommand = new Command<KeyViewModel>(KeyPress);
            ShiftCommand = new Command(ShiftPress);
            CapsCommand = new Command(CapsPress);
            EnterCommand = new Command(EnterPress);
            BackSpaceCommand = new Command(BackSpacePress);
            TabCommand = new Command(TabPress);
            SpaceCommand = new Command(SpacePress);
            HideKeyboardCommand = new Command(OnHideKeyboard);

        }

        private ITranslationProvider _translationProvider;
        public ITranslationProvider TranslationProvider
        {
            get
            {
                return _translationProvider ?? (_translationProvider = IoCContainer.Kernel.Get<ITranslationProvider>());
            }
        }

        public override void OnNavigationCompleted()
        {
            base.OnNavigationCompleted();
        }
        private void OnHideKeyboard()
        {
            IsMouseOver = false;
            OnHideKeyboard("");
        }

        private void OnHideKeyboard(string obj)
        {
            ShowKeyboard = false;
        }

        private void OnShowKeyboard(string obj)
        {
            ShowKeyboard = true;
        }

        

        private void SpacePress()
        {
            Mediator.SendMessage(MsgTag.SetFocus, MsgTag.SetFocus);

            var text = " ";
            var target = System.Windows.Input.Keyboard.FocusedElement;
            var routedEvent = TextCompositionManager.TextInputEvent;

            target.RaiseEvent(
              new TextCompositionEventArgs(
                InputManager.Current.PrimaryKeyboardDevice,
                new TextComposition(InputManager.Current, target, text)) { RoutedEvent = routedEvent }
            );
        }

        private FrameworkElement GetParent(FrameworkElement target)
        {

            if (target.Parent != null)
            {
                return GetParent(target.Parent as FrameworkElement);
            }
            return target;
        }

        private void TabPress()
        {
            var target = System.Windows.Input.Keyboard.FocusedElement as UIElement;
            if (target != null)
                target.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
        }

        private void BackSpacePress()
        {
            Mediator.SendMessage(MsgTag.SetFocus, MsgTag.SetFocus);
            var key = Key.Back;
            var target = System.Windows.Input.Keyboard.FocusedElement;
            var routedEvent = System.Windows.Input.Keyboard.KeyDownEvent;

            target.RaiseEvent(
              new KeyEventArgs(
                System.Windows.Input.Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(target as Visual),
                0,
                key) { RoutedEvent = routedEvent }
            );
        }

        private void EnterPress()
        {
            Mediator.SendMessage("EnterCommand", MsgTag.EnterCommand);
        }

        private void CapsPress()
        {
            IsCapsLock = !IsCapsLock;
            IsShiftLock = false;
        }

        private void ShiftPress()
        {
            IsShiftLock = !IsShiftLock;
            IsCapsLock = false;
        }

        private bool _showKeyboard;
        public bool ShowKeyboard
        {
            get { return _showKeyboard; }
            set
            {
                if (_isMouseOver1 && !value)
                    value = true;
                _showKeyboard = value;
                OnPropertyChanged("ShowKeyboard");
            }
        }

        public bool IsMouseOver
        {
            get { return _isMouseOver1; }
            set
            {
                _isMouseOver1 = value;
                OnPropertyChanged("IsMouseOver");
            }
        }


        public void SetKeyAssignmentsTo(WhichKeyboardLayout whichKeyboardLayout)
        {
            AssignKeys(KeyAssignmentSet.For(whichKeyboardLayout));
        }

        private void KeyPress(KeyViewModel mykey)
        {
            Mediator.SendMessage(MsgTag.SetFocus, MsgTag.SetFocus);
            var text = mykey.CodePoint.ToString();
            var target = System.Windows.Input.Keyboard.FocusedElement;
            var routedEvent = TextCompositionManager.TextInputEvent;

            if (target != null)
                target.RaiseEvent(
                    new TextCompositionEventArgs(
                        InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, target, text)) { RoutedEvent = routedEvent }
                    );

            IsShiftLock = false;
        }

        public void AssignKeys(KeyAssignmentSet keyAssignmentSet)
        {
            //Console.Write("AssignKeys(" + keyAssignmentSet.Language + "), ");
            int n = TheKeyViewModels.Count;
            Debug.Assert(n == keyAssignmentSet.KeyAssignments.Length, "These collections should be the same length.");
            for (int i = 0; i < n; i++)
            {
                KeyViewModel vm = TheKeyViewModels[i];
                KeyAssignment ka = keyAssignmentSet.KeyAssignments[i];
                vm.IsLetter = ka.IsLetter;
                vm.UnshiftedCodePoint = (char)ka.UnshiftedCodePoint;
                vm.ShiftedCodePoint = (char)ka.ShiftedCodePoint;
                if (ka.IsToShowBothGlyphs)
                {
                    // If it's an underscore,
                    if (ka.ShiftedCodePoint == 0x005F)
                    {
                        // double it up because the first one gets eaten
                        string sText = (char)ka.ShiftedCodePoint + Environment.NewLine + (char)ka.UnshiftedCodePoint;
                        string sTextWithUnderscoresDoubled = sText.Replace("_", @"__");
                        vm.Text = sTextWithUnderscoresDoubled;
                    }
                    else
                    {
                        vm.Text = (char)ka.ShiftedCodePoint + Environment.NewLine + (char)ka.UnshiftedCodePoint;
                    }
                }
                else
                {
                    vm.Text = null;
                }
                // Assign the font family, if one is specified..
                if (!String.IsNullOrEmpty(ka.FontFamilyName))
                {
                    // Those assigned to this specific key-assignment, override those assigned to the key-assignment-set.
                    vm.FontFamily = new FontFamily(ka.FontFamilyName);
                }
                else
                {
                    vm.FontFamily = null;
                }
            }
        }

        #endregion

        #region AvailableKeyboardLayouts

        /// <summary>
        /// Get the collection of available keyboard layouts that we can display in this keyboard.
        /// </summary>
        public ObservableCollection<string> AvailableKeyboardLayouts
        {
            get
            {
                if (_availableKeyboardLayouts == null)
                {
                    _availableKeyboardLayouts = new ObservableCollection<string>();
                    _availableKeyboardLayouts.Add("English");
                    _availableKeyboardLayouts.Add("Italian");
                    _availableKeyboardLayouts.Add("German");
                    _availableKeyboardLayouts.Add("Russian");
                    _availableKeyboardLayouts.Add("Turkish");
                    _availableKeyboardLayouts.Add("Serbian");
                }
                return _availableKeyboardLayouts;
            }
        }

        #endregion

        #region SelectedKeyboardLayout

        /// <summary>
        /// Get/set the language setting for this keyboard.
        /// </summary>
        public WhichKeyboardLayout SelectedKeyboardLayout
        {
            get
            {
                //Console.WriteLine("KeyboardViewModel.SelectedKeyboardLayout.get, returning " + _selectedKeyboardLayout);
                return _selectedKeyboardLayout;
            }
            set
            {
                //Console.WriteLine("KeyboardViewModel.SelectedKeyboardLayout.set, from " + _selectedKeyboardLayout + " to " + value.ToString());
                if (value != _selectedKeyboardLayout)
                {
                    _selectedKeyboardLayout = value;
                    ChangeKeyboardLayout();
                    OnPropertyChanged("SelectedKeyboardLayout");
                }
            }
        }

        private void ChangeKeyboardLayout()
        {
            SetKeyAssignmentsTo(SelectedKeyboardLayout);
        }

        #endregion

        public Command<KeyViewModel> KeyPressCommand { get; set; }
        public Command ShiftCommand { get; set; }
        public Command CapsCommand { get; set; }
        public Command EnterCommand { get; set; }
        public Command BackSpaceCommand { get; set; }
        public Command TabCommand { get; set; }
        public Command SpaceCommand { get; set; }
        public Command HideKeyboardCommand { get; set; }

        #region IsCapsLock

        /// <summary>
        /// Get/set whether the VirtualKeyboard is currently is Caps-Lock mode.
        /// </summary>
        public bool IsCapsLock
        {
            get { return _isCapsLock; }
            set
            {
                if (value != _isCapsLock)
                {
                    _isCapsLock = value;
                    OnPropertyChanged("IsCapsLock");
                    NotifyTheIndividualKeys();
                }
            }
        }

        #endregion

        #region IsShiftLock

        /// <summary>
        /// Get/set whether the VirtualKeyboard is currently is Shift-Lock mode.
        /// </summary>
        public bool IsShiftLock
        {
            get { return _isShiftLock; }
            set
            {
                if (value != _isShiftLock)
                {
                    _isShiftLock = value;
                    OnPropertyChanged("IsShiftLock");
                    NotifyTheIndividualKeys();
                }
            }
        }

        #endregion

        #region NotifyTheIndividualKeys

        /// <summary>
        /// Make the individual key-button view models notify their views that their properties have changed.
        /// </summary>
        private void NotifyTheIndividualKeys()
        {
            if (_vms != null)
            {
                foreach (KeyViewModel keyModel in _vms)
                {
                    if (keyModel != null)
                    {
                        keyModel.Notify("Text");
                        keyModel.Notify("ToolTip");
                    }
                }
            }
        }

        #endregion

        #region The view-model properties for the individual key-buttons of the keyboard

        public KeyViewModel VK_OEM_3
        {
            get { return _VK_OEM_3; }
            set { _VK_OEM_3 = value; }
        }

        public KeyViewModel VK_1
        {
            get { return _VK_1; }
            set { _VK_1 = value; }
        }

        public KeyViewModel VK_2
        {
            get { return _VK_2; }
            set { _VK_2 = value; }
        }

        public KeyViewModel VK_3
        {
            get { return _VK_3; }
            set { _VK_3 = value; }
        }

        public KeyViewModel VK_4
        {
            get { return _VK_4; }
            set { _VK_4 = value; }
        }

        public KeyViewModel VK_5
        {
            get { return _VK_5; }
            set { _VK_5 = value; }
        }

        public KeyViewModel VK_6
        {
            get { return _VK_6; }
            set { _VK_6 = value; }
        }

        public KeyViewModel VK_7
        {
            get { return _VK_7; }
            set { _VK_7 = value; }
        }

        public KeyViewModel VK_8
        {
            get { return _VK_8; }
            set { _VK_8 = value; }
        }

        public KeyViewModel VK_9
        {
            get { return _VK_9; }
            set { _VK_9 = value; }
        }

        public KeyViewModel VK_0
        {
            get { return _VK_0; }
            set { _VK_0 = value; }
        }

        public KeyViewModel VK_OEM_MINUS
        {
            get { return _VK_OEM_MINUS; }
            set { _VK_OEM_MINUS = value; }
        }

        public KeyViewModel VK_OEM_PLUS
        {
            get { return _VK_OEM_PLUS; }
            set { _VK_OEM_PLUS = value; }
        }

        public KeyViewModel TabKey
        {
            get { return _Tab; }
            set { _Tab = value; }
        }

        public KeyViewModel VK_Q
        {
            get { return _VK_Q; }
            set { _VK_Q = value; }
        }

        public KeyViewModel VK_W
        {
            get { return _VK_W; }
            set { _VK_W = value; }
        }

        public KeyViewModel VK_E
        {
            get { return _VK_E; }
            set { _VK_E = value; }
        }

        public KeyViewModel VK_R
        {
            get { return _VK_R; }
            set { _VK_R = value; }
        }

        public KeyViewModel VK_T
        {
            get { return _VK_T; }
            set { _VK_T = value; }
        }

        public KeyViewModel VK_Y
        {
            get { return _VK_Y; }
            set { _VK_Y = value; }
        }

        public KeyViewModel VK_U
        {
            get { return _VK_U; }
            set { _VK_U = value; }
        }

        public KeyViewModel VK_I
        {
            get { return _VK_I; }
            set { _VK_I = value; }
        }

        public KeyViewModel VK_O
        {
            get { return _VK_O; }
            set { _VK_O = value; }
        }

        public KeyViewModel VK_P
        {
            get { return _VK_P; }
            set { _VK_P = value; }
        }

        public KeyViewModel VK_OEM_4
        {
            get { return _VK_OEM_4; }
            set { _VK_OEM_4 = value; }
        }

        public KeyViewModel VK_OEM_6
        {
            get { return _VK_OEM_6; }
            set { _VK_OEM_6 = value; }
        }

        public KeyViewModel VK_OEM_5
        {
            get { return _VK_OEM_5; }
            set { _VK_OEM_5 = value; }
        }

        public KeyViewModel VK_A
        {
            get { return _VK_A; }
            set { _VK_A = value; }
        }

        public KeyViewModel VK_S
        {
            get { return _VK_S; }
            set { _VK_S = value; }
        }

        public KeyViewModel VK_D
        {
            get { return _VK_D; }
            set { _VK_D = value; }
        }

        public KeyViewModel VK_F
        {
            get { return _VK_F; }
            set { _VK_F = value; }
        }

        public KeyViewModel VK_G
        {
            get { return _VK_G; }
            set { _VK_G = value; }
        }

        public KeyViewModel VK_H
        {
            get { return _VK_H; }
            set { _VK_H = value; }
        }

        public KeyViewModel VK_J
        {
            get { return _VK_J; }
            set { _VK_J = value; }
        }

        public KeyViewModel VK_K
        {
            get { return _VK_K; }
            set { _VK_K = value; }
        }

        public KeyViewModel VK_L
        {
            get { return _VK_L; }
            set { _VK_L = value; }
        }

        public KeyViewModel VK_OEM_1
        {
            get { return _VK_OEM_1; }
            set { _VK_OEM_1 = value; }
        }

        public KeyViewModel VK_OEM_7
        {
            get { return _VK_OEM_7; }
            set { _VK_OEM_7 = value; }
        }

        public KeyViewModel EnterKey
        {
            get { return _Enter; }
            set { _Enter = value; }
        }

        public KeyViewModel VK_Z
        {
            get { return _VK_Z; }
            set { _VK_Z = value; }
        }

        public KeyViewModel VK_X
        {
            get { return _VK_X; }
            set { _VK_X = value; }
        }

        public KeyViewModel VK_C
        {
            get { return _VK_C; }
            set { _VK_C = value; }
        }

        public KeyViewModel VK_V
        {
            get { return _VK_V; }
            set { _VK_V = value; }
        }

        public KeyViewModel VK_B
        {
            get { return _VK_B; }
            set { _VK_B = value; }
        }

        public KeyViewModel VK_N
        {
            get { return _VK_N; }
            set { _VK_N = value; }
        }

        public KeyViewModel VK_M
        {
            get { return _VK_M; }
            set { _VK_M = value; }
        }

        public KeyViewModel VK_OEM_COMMA
        {
            get { return _VK_OEM_COMMA; }
            set { _VK_OEM_COMMA = value; }
        }

        public KeyViewModel VK_OEM_PERIOD
        {
            get { return _VK_OEM_PERIOD; }
            set { _VK_OEM_PERIOD = value; }
        }

        public KeyViewModel VK_OEM_2
        {
            get { return _VK_OEM_2; }
            set { _VK_OEM_2 = value; }
        }

        public KeyViewModel VK_SPACE
        {
            get { return _VK_SPACE; }
            set { _VK_SPACE = value; }
        }

        public KeyViewModel VK_AT
        {
            get { return _VK_AT; }
            set { _VK_AT = value; }
        }
        #endregion The view-model properties for the individual key-buttons of the keyboard

        #region TheKeyViewModels array

        /// <summary>
        /// Get the array of KeyViewModels that comprise the view-model for the keyboard key-buttons
        /// </summary>
        public ObservableCollection<KeyViewModel> TheKeyViewModels
        {
            get { return _vms; }
        }

        #endregion

        #region fields

        private KeyViewModel _Enter = new KeyViewModel();
        private KeyViewModel _Tab = new KeyViewModel();
        private KeyViewModel _VK_0 = new KeyViewModel();
        private KeyViewModel _VK_1 = new KeyViewModel();
        private KeyViewModel _VK_2 = new KeyViewModel();
        private KeyViewModel _VK_3 = new KeyViewModel();
        private KeyViewModel _VK_4 = new KeyViewModel();
        private KeyViewModel _VK_5 = new KeyViewModel();
        private KeyViewModel _VK_6 = new KeyViewModel();
        private KeyViewModel _VK_7 = new KeyViewModel();
        private KeyViewModel _VK_8 = new KeyViewModel();
        private KeyViewModel _VK_9 = new KeyViewModel();
        private KeyViewModel _VK_A = new KeyViewModel();
        private KeyViewModel _VK_B = new KeyViewModel();
        private KeyViewModel _VK_C = new KeyViewModel();
        private KeyViewModel _VK_D = new KeyViewModel();
        private KeyViewModel _VK_E = new KeyViewModel();
        private KeyViewModel _VK_F = new KeyViewModel();
        private KeyViewModel _VK_G = new KeyViewModel();
        private KeyViewModel _VK_H = new KeyViewModel();
        private KeyViewModel _VK_I = new KeyViewModel();
        private KeyViewModel _VK_J = new KeyViewModel();
        private KeyViewModel _VK_K = new KeyViewModel();
        private KeyViewModel _VK_L = new KeyViewModel();
        private KeyViewModel _VK_M = new KeyViewModel();
        private KeyViewModel _VK_N = new KeyViewModel();
        private KeyViewModel _VK_O = new KeyViewModel();
        private KeyViewModel _VK_OEM_1 = new KeyViewModel();
        private KeyViewModel _VK_OEM_2 = new KeyViewModel();
        private KeyViewModel _VK_OEM_3 = new KeyViewModel();
        private KeyViewModel _VK_OEM_4 = new KeyViewModel();
        private KeyViewModel _VK_OEM_5 = new KeyViewModel();
        private KeyViewModel _VK_OEM_6 = new KeyViewModel();
        private KeyViewModel _VK_OEM_7 = new KeyViewModel();
        private KeyViewModel _VK_OEM_COMMA = new KeyViewModel();
        private KeyViewModel _VK_OEM_MINUS = new KeyViewModel();
        private KeyViewModel _VK_OEM_PERIOD = new KeyViewModel();
        private KeyViewModel _VK_OEM_PLUS = new KeyViewModel();
        private KeyViewModel _VK_P = new KeyViewModel();
        private KeyViewModel _VK_Q = new KeyViewModel();
        private KeyViewModel _VK_R = new KeyViewModel();
        private KeyViewModel _VK_S = new KeyViewModel();
        private KeyViewModel _VK_SPACE = new KeyViewModel();
        private KeyViewModel _VK_T = new KeyViewModel();
        private KeyViewModel _VK_U = new KeyViewModel();
        private KeyViewModel _VK_V = new KeyViewModel();
        private KeyViewModel _VK_W = new KeyViewModel();
        private KeyViewModel _VK_X = new KeyViewModel();
        private KeyViewModel _VK_Y = new KeyViewModel();
        private KeyViewModel _VK_Z = new KeyViewModel();
        private KeyViewModel _VK_AT = new KeyViewModel();
        private ObservableCollection<string> _availableKeyboardLayouts;
        private bool _isCapsLock;
        private bool _isShiftLock;
        private WhichKeyboardLayout _selectedKeyboardLayout = WhichKeyboardLayout.Unknown;
        private ObservableCollection<KeyViewModel> _vms;
        private bool _isMouseOver1;

        #endregion fields

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}