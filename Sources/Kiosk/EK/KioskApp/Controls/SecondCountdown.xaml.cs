using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace KioskApp.Controls
{
    public sealed partial class SecondCountdown : UserControl
    {
        public SecondCountdown()
        {
            InitializeComponent();
        }

        #region Seconds

        public static readonly DependencyProperty SecondsProperty = DependencyProperty.Register(
            nameof(Seconds), typeof(int), typeof(SecondCountdown), new PropertyMetadata(default(int), (o, args) =>
                {
                    var instance = (SecondCountdown)o;
                    if (instance.State == SecondCountdownStateEnum.Stopped)
                    {
                        instance.CurrentSecond = (int)args.NewValue;
                    }
                }));

        public int Seconds
        {
            get { return (int)GetValue(SecondsProperty); }
            set { SetValue(SecondsProperty, value); }
        }

        #endregion

        #region CurrentSecond

        public static readonly DependencyProperty CurrentSecondProperty = DependencyProperty.Register(
            nameof(CurrentSecond), typeof(int), typeof(SecondCountdown), new PropertyMetadata(default(int)));

        public int CurrentSecond
        {
            get { return (int)GetValue(CurrentSecondProperty); }
            set { SetValue(CurrentSecondProperty, value); }
        }

        #endregion

        #region State

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            nameof(State), typeof(SecondCountdownStateEnum), typeof(SecondCountdown), new PropertyMetadata(SecondCountdownStateEnum.Stopped));

        public SecondCountdownStateEnum State
        {
            get { return (SecondCountdownStateEnum)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        #endregion

        #region ArcFill

        public static readonly DependencyProperty ArcFillProperty = DependencyProperty.Register(
            nameof(ArcFill), typeof(Brush), typeof(SecondCountdown), new PropertyMetadata(default(Brush)));

        public Brush ArcFill
        {
            get { return (Brush)GetValue(ArcFillProperty); }
            set { SetValue(ArcFillProperty, value); }
        }

        #endregion

        #region ArcThickness

        public static readonly DependencyProperty ArcThicknessProperty = DependencyProperty.Register(
            nameof(ArcThickness), typeof(double), typeof(SecondCountdown), new PropertyMetadata(default(double)));

        public double ArcThickness
        {
            get { return (double)GetValue(ArcThicknessProperty); }
            set { SetValue(ArcThicknessProperty, value); }
        }

        #endregion

        #region ArcRadius

        public static readonly DependencyProperty ArcRadiusProperty = DependencyProperty.Register(
            nameof(ArcRadius), typeof(double), typeof(SecondCountdown), new PropertyMetadata(default(double)));

        public double ArcRadius
        {
            get { return (double)GetValue(ArcRadiusProperty); }
            set { SetValue(ArcRadiusProperty, value); }
        }

        #endregion

        #region TextMargin

        public static readonly DependencyProperty TextMarginProperty = DependencyProperty.Register(
            nameof(TextMargin), typeof(Thickness), typeof(SecondCountdown), new PropertyMetadata(default(Thickness)));

        public Thickness TextMargin
        {
            get { return (Thickness)GetValue(TextMarginProperty); }
            set { SetValue(TextMarginProperty, value); }
        }

        #endregion

        #region CurrentPercentValue

        public static readonly DependencyProperty CurrentPercentValueProperty = DependencyProperty.Register(
            nameof(CurrentPercentValue), typeof(double), typeof(SecondCountdown), new PropertyMetadata(100.0));

        public double CurrentPercentValue
        {
            get { return (double)GetValue(CurrentPercentValueProperty); }
            set { SetValue(CurrentPercentValueProperty, value); }
        }

        #endregion

        // no need to sync control methods since they are invoked from UI thread
        public void Start()
        {
            if (State == SecondCountdownStateEnum.Running)
            {
                return;
            }

            CurrentSecond = Seconds;
            State = SecondCountdownStateEnum.Running;
#pragma warning disable 4014
            RunTimerAsync();
#pragma warning restore 4014
        }

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(33); // 30 fps

        private DateTime _lastStartedOn;

        // executed in UI thread
        private async Task RunTimerAsync()
        {
            _lastStartedOn = DateTime.Now;
            while (true)
            {
                await Task.Delay(_updateInterval);
                if (State != SecondCountdownStateEnum.Running)
                {
                    break;
                }

                var timeElapsed = DateTime.Now - _lastStartedOn;
                var totalSeconds = timeElapsed.TotalSeconds;
                if (totalSeconds >= Seconds)
                {
                    try
                    {
                        RaiseRunOutEvent();
                    }
                    catch
                    {
                        State = SecondCountdownStateEnum.RunOut;
                        throw;
                    }

                    break;
                }

                CurrentSecond = Seconds - (int)totalSeconds;
                CurrentPercentValue = ((Seconds - totalSeconds)/Seconds)*100;
            }
        }

        public void Stop()
        {
            State = SecondCountdownStateEnum.Stopped;
        }

        public event EventHandler RunOut;

        private void RaiseRunOutEvent()
        {
            RunOut?.Invoke(this, EventArgs.Empty);
        }
    }
}