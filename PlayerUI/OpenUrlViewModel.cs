﻿using Bivrost.Log;
using Caliburn.Micro;
using PlayerUI.Tools;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerUI
{
	public class OpenUrlViewModel : Screen
	{
		private OpenUrlView view;

		public OpenUrlViewModel()
		{
			DisplayName = "Open video URL";
		}

		protected override void OnViewLoaded(object view)
		{
			base.OnViewLoaded(view);
			this.view = view as OpenUrlView;
		}

		private string _url = "";
		public string Url { get { return _url; } set {
				_url = value;
				NotifyOfPropertyChange(() => Url);
			} }

		public Streaming.ServiceResult ServiceResult { get; internal set; }
        public bool Valid { get {
                return ServiceResult != null;
            } }

        public void Open()
		{
            ServiceResult = null;
			Url = Url.Trim();
			if (string.IsNullOrWhiteSpace(Url))
               TryClose();

			if(view == null)
            {
                Process();
                return;
            }

			Task.Factory.StartNew(() =>
			{
				Process();
			});

			view.Open.IsEnabled = false;
			view.Url.IsEnabled = false;
			view.progressBar.Visibility = System.Windows.Visibility.Visible;
        }

		private void Process()
		{
			try
			{
                ServiceResult = null;
                ServiceResult = Streaming.StreamingFactory.Instance.GetStreamingInfo(Url);
			}
			catch(Streaming.StreamNotSupported exc)
			{
                Logger.Error(exc, "Streaming: video not supported. " + Url);
                Execute.OnUIThreadAsync(() =>
					ShellViewModel.Instance.NotificationCenter.PushNotification(new NotificationViewModel("Video not yet supported."))
				);
			}
			catch(Streaming.StreamParsingFailed exc)
			{
                Logger.Error(exc, "Streaming: Parsing failed. Unable to open the video." + Url);
                Execute.OnUIThreadAsync(() =>
					ShellViewModel.Instance.NotificationCenter.PushNotification(new NotificationViewModel("Parsing failed. Unable to open the video."))
				);
			}
			catch (Exception exc)
			{
                Logger.Error(exc, "Streaming: media not supported" + Url);
				Execute.OnUIThreadAsync(() => 
					ShellViewModel.Instance.NotificationCenter.PushNotification(new NotificationViewModel("Media not supported."))
				);
			}
            if (view != null)
                Execute.OnUIThreadAsync(() => TryClose());
		}

	}
}
