using System;
using Avalonia;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls
{
    public class TabViewStackPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            double width = 0;
            double height = 0;
            var children = Children;
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Measure(availableSize);
                width += children[i].DesiredSize.Width;
                height = Math.Max(height, children[i].DesiredSize.Height);
            }

            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_startReorderIndex == -1 && _insertionIndex == -1)
                ArrangeNormal(finalSize);
            else if (_startReorderIndex == -1)
                ArrangeInsertionOnly(finalSize);
            else
                ArrangeWithDragReorder(finalSize);

            return finalSize;
        }

        private void ArrangeNormal(Size finalSize)
        {
            double x = 0;
            Rect rc;
            var children = Children;
            for (int i = 0; i < children.Count; i++)
            {
                rc = new Rect(x, 0, children[i].DesiredSize.Width, finalSize.Height);
                children[i].Arrange(rc);
                x += rc.Width;
            }
        }

        private void ArrangeWithDragReorder(Size finalSize)
        {
            double x = 0;
            Rect rc;
            var children = Children;

            if (_insertionIndex > _startReorderIndex)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (i == _startReorderIndex)
                    {
                        // Arrange offscreen, but DONT advance the rect x
                        rc = new Rect(x, -finalSize.Height - 100, children[i].DesiredSize.Width, finalSize.Height);
                        children[i].Arrange(rc);
                    }
                    else
                    {
                        rc = new Rect(new Point(x, 0), children[i].DesiredSize);
                        children[i].Arrange(rc);
                        x += rc.Width;

                        if (i == _insertionIndex)
                        {
                            // TODO, if insert only, need width
                            x += _startReorderIndex == -1 ? children[0].DesiredSize.Width :
                                children[_startReorderIndex].DesiredSize.Width;
                        }
                    }
                }
            }
            else if (_insertionIndex < _startReorderIndex)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (i < _insertionIndex)
                    {
                        rc = new Rect(new Point(x, 0), children[i].DesiredSize);
                        children[i].Arrange(rc);
                        x += rc.Width;
                    }
                    else if (i == _insertionIndex)
                    {
                        // leave a space
                        x += children[_startReorderIndex].DesiredSize.Width;

                        rc = new Rect(new Point(x, 0), children[i].DesiredSize);
                        children[i].Arrange(rc);
                        x += rc.Width;
                    }
                    else if (i == _startReorderIndex)
                    {
                        // Arrange offscreen, but DONT advance the rect x
                        rc = new Rect(x, -finalSize.Height - 100, children[i].DesiredSize.Width, finalSize.Height);
                        children[i].Arrange(rc);
                    }
                    else
                    {
                        rc = new Rect(new Point(x, 0), children[i].DesiredSize);
                        children[i].Arrange(rc);
                        x += rc.Width;
                    }
                }
            }
            else
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (i == _startReorderIndex)
                    {
                        // Arrange offscreen
                        rc = new Rect(x, -finalSize.Height - 100, children[i].DesiredSize.Width, finalSize.Height);
                        children[i].Arrange(rc);
                        x += rc.Width;
                    }
                    else
                    {
                        rc = new Rect(new Point(x, 0), children[i].DesiredSize);
                        children[i].Arrange(rc);
                        x += rc.Width;
                    }
                }
            }
        }

        private void ArrangeInsertionOnly(Size finalSize)
        {
            double x = 0;
            Rect rc;
            var children = Children;
            for (int i = 0; i < children.Count; i++)
            {
                if (i == _insertionIndex)
                {
                    double wid = 0;
                    if (i == 0)
                    {
                        wid = children[0].DesiredSize.Width;
                    }
                    else
                    {
                        wid = (children[i - 1].DesiredSize.Width + children[i].DesiredSize.Height) / 2;
                    }

                    // Leave a space at the insertion index
                    x += wid;
                }

                rc = new Rect(x, 0, children[i].DesiredSize.Width, finalSize.Height);
                children[i].Arrange(rc);
                x += rc.Width;
            }
        }

        internal void EnterReorder(int startIndex)
        {
            // We're already in a reorder operation, dragenter was probably triggered
            // by a TabViewItem
            if (_insertionIndex != -1)
                return;
            _startReorderIndex = _insertionIndex = startIndex;
            InvalidateArrange();
        }

        internal void ChangeReorderIndex(int index)
        {
            if (_insertionIndex != index)
            {
                if (index == -1)
                    _insertionIndex = _startReorderIndex;

                _insertionIndex = index;
                InvalidateArrange();
            }
        }

        internal int ClearReorder()
        {
            var old = _insertionIndex;
            _startReorderIndex = _insertionIndex = -1;
            InvalidateArrange();
            return old;
        }

        internal int GetInsertionIndexFromPoint(Point p, int estimatedIndex = -1)
        {
            if (_startReorderIndex == -1)
            {
                // This is an insertion operation only, so each item is already at it's
                // logical position, so simple bounds check will work

                if (p.X < Children[0].Bounds.Center.X)
                {
                    return 0;
                }
                else if (p.X > Children[Children.Count - 1].Bounds.Center.X)
                {
                    return Children.Count;
                }
                else
                {
                    for (int i = 1; i < Children.Count; i++)
                    {
                        var x1 = Children[i-1].Bounds.Center.X;
                        var x2 = Children[i].Bounds.Center.X;

                        if (p.X >= x1 && p.X <= x2)
                        {
                            return i;
                        }
                    }
                }
            }

            if (estimatedIndex > -1)
            {
                var initBounds = Children[estimatedIndex].Bounds;

                for (int i = 0; i < Children.Count; i++)
                {
                    if (i == _startReorderIndex)
                        continue;

                    var bnds = Children[i].Bounds;

                    if (bnds.Contains(p))
                    {
                        if (bnds.X < initBounds.X)
                        {
                            if (p.X < bnds.Center.X)
                            {
                                return TranslateIndex(i);
                            }
                        }
                        else if (bnds.X > initBounds.X)
                        {
                            if (p.X > bnds.Center.X)
                            {
                                return TranslateIndex(i);
                            }
                        }
                    }
                }

                return _insertionIndex;// (_insertionIndex == -1) ? estimatedIndex : _insertionIndex;
            }
            else
            {
                // Don't need this now, may in the future
                //throw new NotImplementedException();
                return _insertionIndex;
            }
        }

        private int TranslateIndex(int index)
        {
            if (_insertionIndex == _startReorderIndex)
                return index;

            if (_insertionIndex < _startReorderIndex)
            {
                if (index >= _insertionIndex && index <= _startReorderIndex)
                    return index + 1;
            }
            else if (_insertionIndex > _startReorderIndex)
            {
                if (index <= _insertionIndex && index >= _startReorderIndex)
                    return index - 1;
            }

            return index;
        }

        private int _startReorderIndex = -1;
        private int _insertionIndex = -1;
    }
}
