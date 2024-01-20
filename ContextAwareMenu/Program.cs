// See https://aka.ms/new-console-template for more information

Console.Clear();

var freyaActions = new List<string>
{
    "Attack", "Defend", "Jump",
    "Dragon", "Item", "Change"
};

var zidaneActions = new List<string>
{
    "Attack", "Defend", "Steal",
    "Skill", "Item", "Change"
};

var party = new List<Unit>
{
    new Unit("Freya", 245, 43, 56, 23, freyaActions),
    new Unit("Zidane", 161, 38, 98, 70, zidaneActions)
};

var enemies = new List<Unit>
{
    new Unit("Goblin", 100, 0, 0, 0),
    new Unit("Goblin", 100, 0, 0, 0),
    new Unit("Goblin", 100, 0, 0, 0),
    new Unit("Goblin", 100, 0, 0, 0)
};

var menuContext = new BattleMenuContext(party, enemies);

while (true)
{
    ConsoleKeyInfo key = Console.ReadKey();
    if (key.Key == ConsoleKey.DownArrow) menuContext.PressDown();
    else if (key.Key == ConsoleKey.UpArrow) menuContext.PressUp();
    else if (key.Key == ConsoleKey.RightArrow) menuContext.PressRight();
    else if (key.Key == ConsoleKey.LeftArrow) menuContext.PressLeft();
    else if (key.Key == ConsoleKey.Enter) menuContext.PressEnter();
}

public class BattleMenuContext
{
    public IEnumerable<Unit> Party { get; }
    public IEnumerable<Unit> Enemies { get; }
    public string? ActionSelected { get; set; }
    public Unit TakingTurn { get; set; }
    public Unit? Target { get; set; }

    public (int Left, int Top) CurPos;
    private IMenuState _state;
    private BattleInfoPanel _battleInfoPanel; 
    private ActionSummaryPanel _actionSummaryPanel;
    private int _takingTurnIndex;

    public BattleMenuContext(
        IEnumerable<Unit> party,
        IEnumerable<Unit> enemies)
    {
        Party = party;
        Enemies = enemies;
        TakingTurn = Party.ToArray()[_takingTurnIndex++];

        _state = new SelectActionState(this);
        _state.Display();
        Console.CursorVisible = false;
        _actionSummaryPanel = new ActionSummaryPanel(this);
        _battleInfoPanel = new BattleInfoPanel(this);
        _battleInfoPanel.Display();
    }

    public void ChangeState(IMenuState state)
    {
        state.Clear();
        CurPos = (0, 0);
        _state = state;
        _state.Display();
        _battleInfoPanel.Display();
    }

    public void PressDown() => _state.PressDown();

    public void PressUp() => _state.PressUp();

    public void PressEnter() => _state.PressEnter();

    public void PressRight() => _state.PressRight();

    public void PressLeft() => _state.PressLeft();

    internal void DrawCursor()
    {
        Console.SetCursorPosition(CurPos.Left, CurPos.Top);
        Console.WriteLine(">");
    }

    internal void RemoveCursor()
    {
        Console.SetCursorPosition(CurPos.Left, CurPos.Top);
        Console.Write(" ");
    }

    internal void MoveCursor(MoveDirection direction)
    {
        if (IsOutOfBounds(direction))
        {
            return;
        }

        if (direction == MoveDirection.Down) CurPos.Top++;
        else if (direction == MoveDirection.Up) CurPos.Top--;
        else if (direction == MoveDirection.Right) CurPos.Left += 9;
        else if (direction == MoveDirection.Left) CurPos.Left -= 9;
    }

    private bool IsOutOfBounds(MoveDirection direction)
    {
        if (_state.TopMin == CurPos.Top && direction == MoveDirection.Up)
        {
            return true;
        }

        if (_state.TopMax == CurPos.Top && direction == MoveDirection.Down)
        {
            return true;
        }

        if (_state.LeftMin == CurPos.Left && direction == MoveDirection.Left)
        {
            return true;
        }

        if (_state.LeftMax == CurPos.Left && direction == MoveDirection.Right)
        {
            return true;
        }

        return false;
    }


    public void ExecuteCommand()
    {
        if (Target is null)
        {
            throw new InvalidOperationException(
                "Can't execute command without a target.");
        }
        
        if (ActionSelected == "Attack")
        {
            Target.CurrentHp--;
            // Update action summary panel
            _actionSummaryPanel.Display();
            
            // Advance turn to next unit
            TakingTurn = Party.ToArray()[_takingTurnIndex++ % Party.Count()];
            
            // Update battle info panel
            _battleInfoPanel.Display();
        }
        else
        {
            throw new InvalidOperationException($"Command {ActionSelected} not found.");
        }
    }
}

internal class ActionSummaryPanel
{
    private const int TopOffset = 6;
    private readonly BattleMenuContext _ctx;

    public ActionSummaryPanel(BattleMenuContext ctx)
    {
        _ctx = ctx;
    }

    public void Display()
    {
        Console.SetCursorPosition(0, TopOffset);
        Console.Write(_ctx.TakingTurn.Name + " " + _ctx.ActionSelected + " " + _ctx.Target?.Name);
        Thread.Sleep(2000);
    }
}

internal class BattleInfoPanel
{
    private readonly IEnumerable<Unit> _party;
    private readonly BattleMenuContext _ctx;
    private const int LeftOffset = 40;

    public BattleInfoPanel(BattleMenuContext ctx)
    {
        _ctx = ctx;
        _party = ctx.Party;
    }

    public void Display()
    {
        for (int i = 0; i < _party.Count(); i++)
        {
            Unit unit = _party.ToArray()[i];

            if (unit.Name == _ctx.TakingTurn.Name)
            {
                Console.SetCursorPosition(LeftOffset, i);
                Console.Write("+");
            }

            Console.SetCursorPosition(LeftOffset + 1, i);
            Console.Write(unit.Name);
            Console.SetCursorPosition(LeftOffset + 10, i);
            Console.Write(unit.CurrentHp);
            Console.SetCursorPosition(LeftOffset + 15, i);
            Console.Write(unit.CurrentMp);
            Console.SetCursorPosition(LeftOffset + 20, i);
            Console.Write(unit.CurrentAtb);
            Console.SetCursorPosition(LeftOffset + 25, i);
            Console.Write(unit.CurrentTrance);
        }
    }
}

public class SelectActionState : IMenuState
{
    private readonly BattleMenuContext _ctx;

    public SelectActionState(BattleMenuContext ctx)
    {
        _ctx = ctx;
    }

    public void PressDown() => MoveCursor(MoveDirection.Down);

    private void MoveCursor(MoveDirection direction)
    {
        _ctx.RemoveCursor();
        _ctx.MoveCursor(direction);
        _ctx.DrawCursor();
    }

    public void PressUp() => MoveCursor(MoveDirection.Up);

    public void PressRight() => MoveCursor(MoveDirection.Right);

    public void PressLeft() => MoveCursor(MoveDirection.Left);

    public void PressEnter()
    {
        int index = _ctx.CurPos.Top * 2 ;
        index += _ctx.CurPos.Left == 9 ? 1 : 0;
        
        _ctx.ActionSelected = _ctx.TakingTurn.Actions.ToArray()[index];
        _ctx.ChangeState(new SelectTargetState(_ctx));
    }

    public void Display()
    {
        List<string>? takingTurnActions = _ctx.TakingTurn.Actions;
        for (int i = 0; i < takingTurnActions.Count; i++)
        {
            if (i % 2 == 0)
            {
                Console.Write("  " + takingTurnActions[i].PadRight(9));
            }
            else
            {
                Console.Write(takingTurnActions[i].PadLeft(2) + "\r\n");
            }
        }

        _ctx.DrawCursor();
    }

    public void Clear()
    {
        Console.Clear();
    }

    public int TopMin => 0;
    public int TopMax => 2;
    public int LeftMin => 0;
    public int LeftMax => 9;
}

internal enum MoveDirection
{
    Up,
    Down,
    Right,
    Left
}

public class SelectTargetState : IMenuState
{
    private readonly BattleMenuContext _ctx;

    public SelectTargetState(BattleMenuContext ctx)
    {
        _ctx = ctx;
    }

    public void PressDown() => MoveCursor(MoveDirection.Down);

    private void MoveCursor(MoveDirection direction)
    {
        _ctx.RemoveCursor();
        _ctx.MoveCursor(direction);
        _ctx.DrawCursor();
    }

    public void PressUp() => MoveCursor(MoveDirection.Up);

    public void PressRight() => MoveCursor(MoveDirection.Right);

    public void PressLeft() => MoveCursor(MoveDirection.Left);

    public void PressEnter()
    {
        _ctx.Target = _ctx.CurPos.Left == 0
            ? _ctx.Enemies.ToArray()[_ctx.CurPos.Top]
            : _ctx.Party.ToArray()[_ctx.CurPos.Top];

        _ctx.ExecuteCommand();
        _ctx.ChangeState(new SelectActionState(_ctx));
    }

    public void Display()
    {
        // On the left side place enemies
        // On the right side place party members
        Unit unit;
        for (int i = 0; i < 4; i++)
        {
            Console.SetCursorPosition(0, i);
            // Place enemy
            if (i < _ctx.Enemies.Count())
            {
                unit = _ctx.Enemies.ToArray()[i];
                Console.Write("  " + unit.Name.PadRight(9));
            }
            else
            {
                Console.Write("         ");
            }

            // Places party member
            if (i < _ctx.Party.Count())
            {
                unit = _ctx.Party.ToArray()[i];
                Console.Write(unit.Name.PadLeft(2) + "\r\n");
            }
        }

        _ctx.DrawCursor();
    }

    public void Clear()
    {
        Console.Clear();
    }

    public int TopMin => 0;

    public int TopMax
    {
        get
        {
            if (_ctx.CurPos.Left == 0)
            {
                return _ctx.Enemies.Count() - 1;
            }

            return _ctx.Party.Count() - 1;
        }
    }

    public int LeftMin => 0;

    public int LeftMax
    {
        get
        {
            if (_ctx.CurPos.Top < _ctx.Party.Count())
            {
                return 9;
            }

            return 0;
        }
    }
}

public interface IMenuState
{
    void PressDown();
    void PressUp();
    void PressRight();
    void PressLeft();
    void PressEnter();
    void Display();
    void Clear();

    /// <summary>
    ///     The minimum top position for the cursor to be at.
    /// </summary>
    int TopMin { get; }

    /// <summary>
    ///     The maximum top position for the cursor to be at.
    /// </summary>
    int TopMax { get; }

    /// <summary>
    ///     The minimum left position for the cursor to be at.
    /// </summary>
    int LeftMin { get; }

    /// <summary>
    ///     The maximum left position for the cursor to be at.
    /// </summary>
    int LeftMax { get; }
}

public class Unit
{
    public string Name { get; }
    public int CurrentHp { get; set; }
    public int CurrentMp { get; set; }
    public int CurrentAtb { get; set; }
    public int CurrentTrance { get; set; }
    public List<string>? Actions { get; }

    public Unit(string name)
    {
        Name = name;
    }

    public Unit(string name,
        int currentHp,
        int currentMp,
        int currentAtb,
        int currentTrance,
        List<string> actions = null!)
    {
        Name = name;
        CurrentHp = currentHp;
        CurrentMp = currentMp;
        CurrentAtb = currentAtb;
        CurrentTrance = currentTrance;
        Actions = actions;
    }
}