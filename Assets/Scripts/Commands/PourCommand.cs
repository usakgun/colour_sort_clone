public class PourCommand : ICommand
{
    private readonly BottleModel _source;
    private readonly BottleModel _destination;
    private int _pouredCount;
    private ColorType _pouredColor;

    public PourCommand(BottleModel source, BottleModel destination)
    {
        _source = source;
        _destination = destination;
    }

    public bool Execute()
    {
        if (_source.IsEmpty || _destination.IsFull) return false;
        if (!_destination.IsEmpty && _source.TopColor != _destination.TopColor) return false;

        _pouredColor = _source.TopColor;
        _pouredCount = 0;

        while (!_source.IsEmpty && !_destination.IsFull && _source.TopColor == _pouredColor)
        {
            _source.RemoveColor();
            _destination.AddColor(_pouredColor);
            _pouredCount++;
        }

        return _pouredCount > 0;
    }

    public void Undo()
    {
        for (int i = 0; i < _pouredCount; i++)
        {
            _destination.RemoveColor();
            _source.ForceAddColor(_pouredColor);
        }
    }
}