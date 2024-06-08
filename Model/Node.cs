using MudBlazor;

namespace Snake.Model
{
    public class Node
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public bool IsBody = false;
        public bool IsfoodPosition = false;
        public string NodeColor = $"background:{Colors.LightBlue.Lighten5};";
        public Node(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Node Child { get; set; }
    }
}
