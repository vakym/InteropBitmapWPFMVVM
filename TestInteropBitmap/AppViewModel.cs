using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestInteropBitmap
{
    public class AppViewModel : BaseViewModel
    {
      private GrabberClass grabber;
      public GrabberClass Grabber { get { return grabber; } set { grabber = value; OnPropertyChanged(); } }
      private bool state;
      public bool State
      {
            get
            {
                return state;
            }
            set
            {
                state = value;
                OnPropertyChanged();
            }
      }
        private string url;
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
                OnPropertyChanged();
            }
        }
      public Command Start
      {
            get
            {
                return new Command((o) => {
                  Grabber = new GrabberClass();
                  State = Grabber.StartStream(url);
                });
            }
       }
        public Command Stop
        {
            get
            {
                return new Command((o) => {
                   
                    Grabber.StopStream();
                    State = false;
                });
            }
        }

    }
}
