using Avalonia;

namespace FluentAvalonia.UI.Controls;

internal class FlowLayoutState
{
    internal FlowLayoutAlgorithm FlowAlgorithm => _flowAlgorithm;

    internal Size SpecialElementDesiredSize { get; set; }

    internal double TotalLineSize => _totalLineSize;

    internal int TotalLinesMeasured => _totalLinesMeasured;

    internal double TotalItemsPerLine => _totalItemsPerLine;

    public void InitializeForContext(VirtualizingLayoutContext context,
        IFlowLayoutAlgorithmDelegates callbacks)
    {
        _flowAlgorithm.InitializeForContext(context, callbacks);

        if (_lineSizeEstimationBuffer == null)
        {
            _lineSizeEstimationBuffer = new double[BufferSize];
            _itemsPerLineEstimationBuffer = new double[BufferSize];
        }

        context.LayoutStateCore = this;
    }

    public void UninitializeForContext(VirtualizingLayoutContext context)
    {
        _flowAlgorithm.UninitializeForContext(context);
    }

    public void OnLineArranged(int startIndex, int countInLine, double lineSize, VirtualizingLayoutContext context)
    {
        // If we do not have any estimation information, use the line for estimation. 
        // If we do have some estimation information, don't account for the last line which is quite likely
        // different from the rest of the lines and can throw off estimation.
        if (_totalLinesMeasured == 0 || startIndex + countInLine != context.ItemCount)
        {
            int estimationBufferIndex = startIndex % _lineSizeEstimationBuffer.Length;
            bool alreadyMeasured = _lineSizeEstimationBuffer[estimationBufferIndex] != 0;

            if (!alreadyMeasured)
            {
                ++_totalLinesMeasured;
            }

            _totalLineSize -= _lineSizeEstimationBuffer[estimationBufferIndex];
            _totalLineSize += lineSize;
            _lineSizeEstimationBuffer[estimationBufferIndex] = lineSize;

            _totalItemsPerLine -= _itemsPerLineEstimationBuffer[estimationBufferIndex];
            _totalItemsPerLine += countInLine;
            _itemsPerLineEstimationBuffer[estimationBufferIndex] = countInLine;
        }
    }

    private readonly FlowLayoutAlgorithm _flowAlgorithm = new FlowLayoutAlgorithm();
    private double[] _lineSizeEstimationBuffer;
    private double[] _itemsPerLineEstimationBuffer;
    private double _totalLineSize;
    private int _totalLinesMeasured;
    private double _totalItemsPerLine;
    private const int BufferSize = 100;
}
