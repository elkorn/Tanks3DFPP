using Microsoft.Xna.Framework;

namespace Tanks3DFPP.Menu
{
    internal class MenuOption
    {
        public readonly string Text;

        public readonly int Index;

        public bool IsSelected { get; private set; }

        private readonly Vector3 position;

        public MenuOption(string text, int index)
        {
            this.Text = text;
            this.Index = index;
            this.position = new Vector3(200, 100 - 330*this.Index, 0);
        }

        public void Select()
        {
            this.IsSelected = true;
        }

        public void Deselect()
        {
            this.IsSelected = false;
        }

        public void Draw(Characters characters, Matrix view, Matrix projection)
        {
            characters.Draw(this.Text, this.IsSelected ? .9f : .7f, this.position, view, projection);
        }
    }
}
