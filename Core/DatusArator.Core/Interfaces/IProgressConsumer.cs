namespace DatusArator.Core.Interfaces {
  public interface IProgressConsumer {
    void UpdateProgress(string caption, int progress, int max = 100);
  }
}
