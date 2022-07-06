using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 贪吃蛇
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        const int CellSize = 20;                // 小格子大小
        const int SnakeHead = 0;                // 蛇头位置（永远位于列表0）
        const int CellWidth = 640 / CellSize;    // 游戏区横格数
        const int CellHeight = 480 / CellSize;    // 游戏区纵格数
                                                  // 蛇身前进方向
        enum Direction//枚举，限定取值内容和范围
        {
            UP,
            DOWN,
            LEFT,
            RIGHT
        }
        Direction Direct = Direction.UP;//初始化

        // 游戏状态
        enum GameState
        {
            NONE,
            GAMEING,
            PAUSE,
            STOP
        }
        GameState CurrGameState = GameState.NONE;
        List<SnakeNode> SnakeNodes = new List<SnakeNode>();        // 蛇身列表
        Fruit fruit;                                            // 水果初始化
        Random rnd = new Random((int)DateTime.Now.Ticks);        // 随机数
        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();    // 计时器，实现动态过程
        public MainWindow()
        {

            InitializeComponent();

            DrawGrid();
            StartGame();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 260);//天，时，分，秒，毫秒
           timer.Tick += Timer_Tick;  
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CheckCollision();
            GenNewSnakeNode();
            if (IsGameOver())
            {
                CurrGameState = GameState.STOP;
                timer.Stop();
                MessageBox.Show("游戏结束", "警告", MessageBoxButton.OK);

                return;
            }

        }


        //游戏区暗格线 
        //主要使用path控件，通过循环每隔20px画一个横线和竖线
        //ceomerty="M0,0,L1,01,0.1,0,0.1z"  M是移动命令，L是直线命令，z是结束命令
        //其中数值都是比例的意思
        //（0，0）->(1,0)->(1,0.1)->(0,0.1)画一个矩形
        //横坐标的1是1*50=50，0是0*50=0，0.1就是0.1*50=5 纵坐标以此类推。（矩形viewport="0,0,50,50"）
        private void DrawGrid()
        {
            Path gridPath = new Path();//划线
            gridPath.Stroke = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));//颜色
            gridPath.StrokeThickness = 1;//线框

            StringBuilder data = new StringBuilder();

            for (int x = 0; x < 640; x += CellSize)
            {
                data.Append($"M{x},0 L{x},480 ");
            }

            for (int y = 0; y < 480; y += CellSize)
            {
                data.Append($"M0,{y} L640,{y} ");
            }

            gridPath.Data = Geometry.Parse(data.ToString());
            myCanvas.Children.Add(gridPath);//可见，添加到画布里
        }
        //随机水果位置
        private Point SetFruitToRandomPos()
        {
            bool flag = true;
            Point pos = new Point();
            while (flag)
            {
                flag = false;
                pos = new Point(rnd.Next(0, CellWidth), rnd.Next(0, CellHeight));//位置随机生成
                foreach (var node in SnakeNodes)
                {
                    if (pos.X == node._pos.X && pos.Y == node._pos.Y)//防止水果位置与蛇身位置重叠，和x，y轴对比
                    {
                        flag = true;
                        break;
                    }
                }
            }

            return pos;
        }
        //新的单节蛇身
        private void GenNewSnakeNode()
        {
            SnakeNode snakeNode = null;
            switch (Direct)
            {
                case Direction.UP:
                    snakeNode = new SnakeNode(new Point(SnakeNodes[SnakeHead]._pos.X,
                        SnakeNodes[SnakeHead]._pos.Y - 1));
                    break;

                case Direction.DOWN:
                    snakeNode = new SnakeNode(new Point(SnakeNodes[SnakeHead]._pos.X,
                        SnakeNodes[SnakeHead]._pos.Y + 1));
                    break;

                case Direction.LEFT:
                    snakeNode = new SnakeNode(new Point(SnakeNodes[SnakeHead]._pos.X - 1,
                        SnakeNodes[SnakeHead]._pos.Y));
                    break;

                case Direction.RIGHT:
                    snakeNode = new SnakeNode(new Point(SnakeNodes[SnakeHead]._pos.X + 1,
                        SnakeNodes[SnakeHead]._pos.Y));
                    break;
            }

            if (snakeNode != null)
            {
                SnakeNodes.Insert(0, snakeNode);//新生成的蛇头放入列表中
                myCanvas.Children.Add(SnakeNodes[0]._rect);//放入运行区
            }
        }
        //碰撞检测（蛇头和水果）
        //只有蛇头会先碰撞到水果，检查蛇头坐标与水果坐标即可，碰到水果，水果生成新位置，没有碰到，删除蛇尾
        private void CheckCollision()
        {
            if (SnakeNodes[SnakeHead]._pos.X == fruit._pos.X && SnakeNodes[SnakeHead]._pos.Y == fruit._pos.Y)
            {
                fruit.SetPostion(SetFruitToRandomPos());
            }
            else
            {
                if (myCanvas.Children.Contains(SnakeNodes[SnakeNodes.Count - 1]._rect))
                    myCanvas.Children.Remove(SnakeNodes[SnakeNodes.Count - 1]._rect);

                SnakeNodes.RemoveAt(SnakeNodes.Count - 1);
            }
        }
        //判断游戏结束
        private bool IsGameOver()
        {
            if (SnakeNodes[SnakeHead]._pos.X == -1 || SnakeNodes[SnakeHead]._pos.X == CellWidth
                || SnakeNodes[SnakeHead]._pos.Y == -1 || SnakeNodes[SnakeHead]._pos.Y == CellHeight)
            {
                return true;
            }

            foreach (var node in SnakeNodes)
            {
                if (node == SnakeNodes[SnakeHead])
                    continue;

                if (node._pos.X == SnakeNodes[SnakeHead]._pos.X && node._pos.Y == SnakeNodes[SnakeHead]._pos.Y)
                {
                    return true;
                }
            }

            return false;
        }
        //删除游戏区的所有蛇身
        private void RemoveSnakeNodeAll()
        {
            for (int i = 0; i < SnakeNodes.Count; i++)
            {
                if (myCanvas.Children.Contains(SnakeNodes[i]._rect))
                {
                    myCanvas.Children.Remove(SnakeNodes[i]._rect);
                }
            }
        }
        //删除游戏区的水果
        private void RemoveFruit()
        {
            if (fruit == null)
            {
                return;
            }

            if (myCanvas.Children.Contains(fruit._ellipse))
            {
                myCanvas.Children.Remove(fruit._ellipse);
            }
        }
        //游戏开始的方法
        private void StartGame()
        {
            RemoveSnakeNodeAll();
            RemoveFruit();//删除

            int startX = rnd.Next(5, CellWidth - 6);
            int startY = rnd.Next(5, CellHeight - 6);//随机坐标
            Direct = Direction.RIGHT;//开始行走的方向

            fruit = new Fruit(SetFruitToRandomPos(), myCanvas);

            SnakeNodes = new List<SnakeNode>();//储存蛇身列表
            SnakeNodes.Add(new SnakeNode(new Point(startX, startY)));//生成蛇的身节
            GenNewSnakeNode();
            GenNewSnakeNode();//2个身节
        }


      
        
        private void MenuFile_NewGame_Click_1(object sender, RoutedEventArgs e)
        {
            StartGame();
            timer.Start();
            CurrGameState = GameState.GAMEING;
            MenuControl_Pause.Header = "暂停";
        }
        //键盘控制移动
        private void myCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    if (Direct != Direction.RIGHT)
                    {
                        Direct = Direction.LEFT;
                    }
                    break;

                case Key.Right:
                    if (Direct != Direction.LEFT)
                    {
                        Direct = Direction.RIGHT;
                    }
                    break;

                case Key.Up:
                    if (Direct != Direction.DOWN)
                    {
                        Direct = Direction.UP;
                    }
                    break;

                case Key.Down:
                    if (Direct != Direction.UP)
                    {
                        Direct = Direction.DOWN;
                    }
                    break;

                case Key.Escape:
                    Application.Current.Shutdown();
                    break;

                case Key.Space:
                    if (CurrGameState == GameState.NONE)
                        return;

                    if (CurrGameState == GameState.PAUSE)
                    {
                        CurrGameState = GameState.GAMEING;
                        timer.Start();
                        MenuControl_Pause.Header = "暂停";
                    }
                    else if (CurrGameState == GameState.GAMEING)
                    {
                        CurrGameState = GameState.PAUSE;
                        timer.Stop();
                        MenuControl_Pause.Header = "继续";
                    }
                    break;
            }
        }
        //暂停
        private void MenuControl_Pause_Click(object sender, RoutedEventArgs e)
        {
            if (CurrGameState == GameState.GAMEING)
            {
                CurrGameState = GameState.PAUSE;
                timer.Stop();
                MenuControl_Pause.Header = "继续";
            }
            else if (CurrGameState == GameState.PAUSE)
            {
                CurrGameState = GameState.GAMEING;
                timer.Start();
                MenuControl_Pause.Header = "暂停";
            }
        }
        //退出
        private void MenuFile_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
    public class Fruit
    {
        public Point _pos { get; set; }
        public Ellipse _ellipse { get; set; }//椭圆
        public Canvas _canvas { get; set; }

        public Fruit(Point point, Canvas canvas)
        {
            _pos = point;
            _canvas = canvas;

            _ellipse = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.Red
            };

            _ellipse.SetValue(Canvas.LeftProperty, _pos.X * 20);
            _ellipse.SetValue(Canvas.TopProperty, _pos.Y * 20);//定位
            _canvas.Children.Add(_ellipse);
        }

        public void SetPostion(Point pos)//位置
        {
            _pos = pos;

            _ellipse.SetValue(Canvas.LeftProperty, _pos.X * 20);
            _ellipse.SetValue(Canvas.TopProperty, _pos.Y * 20);
        }
    }
    public class SnakeNode
    {
        public Point _pos { get; set; }
        public Rectangle _rect { get; set; }

        public SnakeNode(Point point)
        {
            _pos = point;

            _rect = new Rectangle
            {
                Width = 20,
                Height = 20,
                Stroke = new SolidColorBrush(Colors.DodgerBlue),
                StrokeThickness = 3,
                Fill = Brushes.SkyBlue
            };

            _rect.SetValue(Canvas.LeftProperty, _pos.X * 20);
            _rect.SetValue(Canvas.TopProperty, _pos.Y * 20);
        }
    }

}
