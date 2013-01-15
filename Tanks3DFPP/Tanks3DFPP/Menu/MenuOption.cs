using Microsoft.Xna.Framework;

namespace Tanks3DFPP.Menu
{
    internal class MenuOption
    {
        public readonly string Text;

        public readonly int Index;

        public bool IsSelected { get; protected set; }

        protected readonly Vector3 position;

        protected readonly float baseScale;

        private float actualScale
        {
            get
            {
                return this.IsSelected
                           ? baseScale * 1.2f
                           : baseScale;
            }
        }

        public MenuOption(string text, int index, Vector2 position, float baseScale = .7f)
        {
            this.Text = text;
            this.Index = index;
            this.position = new Vector3(position, 0);
            this.baseScale = baseScale;
        }

        public void Select()
        {
            this.IsSelected = true;
        }

        public void Deselect()
        {
            this.IsSelected = false;
        }

        public virtual void Update()
        {
        }

        public virtual void Draw(Characters characters, Matrix view, Matrix projection)
        {
            characters.Draw(this.Text, this.actualScale, this.position, view, projection);
        }
    }
}
