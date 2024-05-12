namespace AutoRetainer.UI.NeoUI.Experiments;
public abstract class ExperimentUIEntry : NeoUIEntry
{
		public virtual string Name => this.GetType().Name;
		public override string Path => $"Experiments/{Name}";
}
