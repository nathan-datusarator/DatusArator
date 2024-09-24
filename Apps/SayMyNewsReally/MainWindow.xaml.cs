using System;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Windows;

namespace SayMyNewsReally {
  public partial class MainWindow : Window {
    private SpeechSynthesizer synth;

    public MainWindow() {
      InitializeComponent();
    }

    private void btnGo_Click(object sender, RoutedEventArgs e) {
      if (synth == null) {
        synth = new SpeechSynthesizer();

        //foreach (var voice in synth.GetInstalledVoices()) {
        //  var info = voice.VoiceInfo;
        //  Console.WriteLine($"Id: {info.Id} | Name: {info.Name} | Age: {info.Age} | Gender: {info.Gender} | Culture: {info.Culture}");
        //}

        synth.SelectVoice("Microsoft Zira Desktop");
        synth.StateChanged += new EventHandler<StateChangedEventArgs>(synth_StateChanged);
      }

      if (synth.State == SynthesizerState.Paused) {
        synth.Resume();
      } else if (synth.State == SynthesizerState.Speaking) {
        synth.Pause();
      } else {
        synth.SpeakAsync(txtMain.Text);
      }
    }

    void synth_StateChanged(object sender, StateChangedEventArgs e) {
      if (synth.State == SynthesizerState.Ready)
        btnGo.Content = "Go";
      else if (synth.State == SynthesizerState.Paused)
        btnGo.Content = "Resume";
      else if (synth.State == SynthesizerState.Speaking)
        btnGo.Content = "Pause";
    }

    private void btnPaste_Click(object sender, RoutedEventArgs e) {
      IDataObject iData = Clipboard.GetDataObject();
      if (iData.GetDataPresent(DataFormats.Text)) {
        txtMain.Text = (String)iData.GetData(DataFormats.Text);
      }
    }
  }
}
