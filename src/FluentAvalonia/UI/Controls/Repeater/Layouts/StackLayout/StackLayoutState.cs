namespace FluentAvalonia.UI.Controls;

internal class StackLayoutState
{
    public FlowLayoutAlgorithm FlowAlgorithm { get; private set; }

    public double TotalElementSize { get; private set; }

    public double MaxArrangeBounds { get; private set; }

    public int TotalElementsMeasured { get; private set; }

    public void InitializeForContext(VirtualizingLayoutContext context, IFlowLayoutAlgorithmDelegates callbacks)
    {
        FlowAlgorithm ??= new FlowLayoutAlgorithm();
        FlowAlgorithm.InitializeForContext(context, callbacks);

        if (_estimationBuffer == null)
            _estimationBuffer = new double[BufferSize];

        context.LayoutStateCore = this;
    }

    public void UninitializeForContext(VirtualizingLayoutContext context) =>
        FlowAlgorithm.UninitializeForContext(context);

    public void OnElementMeasured(int elementIndex, double majorSize, double minorSize)
    {
        int estimationBufferIndex = elementIndex < BufferSize ? elementIndex :
            elementIndex % BufferSize;
        bool alreadyMeasured = _estimationBuffer[estimationBufferIndex] != 0;
        if (!alreadyMeasured)
        {
            TotalElementsMeasured++;
        }

        TotalElementSize -= _estimationBuffer[estimationBufferIndex];
        TotalElementSize += majorSize;
        _estimationBuffer[estimationBufferIndex] = majorSize;

        MaxArrangeBounds = Math.Max(MaxArrangeBounds, minorSize);
    }

    public void OnMeasureStart()
    {
        MaxArrangeBounds = 0;
    }

    private double[] _estimationBuffer;
    private const int BufferSize = 100;
}
