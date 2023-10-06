namespace AutoRetainer.UI
{
    internal class LogWindow : Window
    {
        public LogWindow() : base("AutoRetainer log")
        {
            this.SizeConstraints = new()
            {
                MinimumSize = new(200, 200),
                MaximumSize = new(float.MaxValue, float.MaxValue)
            };
        }

        public override void Draw()
        {
            InternalLog.PrintImgui();
        }
    }
}
