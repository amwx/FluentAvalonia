using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentAvalonia.UI.Controls
{
    //Used to assist switching Pseudoclasses for the SplitButton & ToggleSplitButton
    internal class TempVisualStateManager : AvaloniaObject
    {
        public static readonly AttachedProperty<IList<TempVisualStateGroup>> VisualStateGroupsProperty =
            AvaloniaProperty.RegisterAttached<TempVisualStateManager, IControl, IList<TempVisualStateGroup>>("VisualStateGroups");

        public static IList<TempVisualStateGroup> GetVisualStateGroups(IControl host)
        {
            var ret = host.GetValue(VisualStateGroupsProperty);
            if (ret == null)
            {
                ret = new List<TempVisualStateGroup>();
                SetVisualStateGroups(host, ret);
            }

            return ret;
        }

        public static void SetVisualStateGroups(IControl host, IList<TempVisualStateGroup> groups)
        {
            host.SetValue(VisualStateGroupsProperty, groups);
        }

        internal static IList<TempVisualStateGroup> GetVisualStateGroupsInternal(IControl host)
        {
            return host.GetValue(VisualStateGroupsProperty);
        }

        public static bool GoToState(IControl control, string stateName)
        {
            return GoToState(control, null, stateName);
        }

        public static bool GoToState(IControl control, string groupName, string stateName)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            //Unlike UWP/WPF, VSM related isn't declared in xaml
            //It's too verbose and annoying to type out
            //So it's all handled here
            //If we pass in group or state that doesn't exist, it's
            //added to the collection
            //States & groups are case-insensitive
            //Group can also be null, indicating base/common states

            var groups = control.GetValue(TempVisualStateManager.VisualStateGroupsProperty);
            if (groups == null)
            {
                groups = new List<TempVisualStateGroup>();
                control.SetValue(VisualStateGroupsProperty, groups);
            }

            if (TryGetGroup(groups, groupName, out TempVisualStateGroup group))
            {
                return TrySetState(group, stateName, control);
            }
            else
            {
                groups.Add(group);
                return TrySetState(group, stateName, control);
            }
        }

        /// <summary>
        /// Gets a VisualStateGroup for a control
        /// </summary>
        /// <returns>True if group already existed, False if group was created</returns>
        public static bool TryGetGroup(IList<TempVisualStateGroup> groups, string groupName, out TempVisualStateGroup group)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                if (string.Equals(groupName, groups[i].GroupName, StringComparison.OrdinalIgnoreCase))
                {
                    group = groups[i];
                    return true;
                }
            }

            group = new TempVisualStateGroup() { GroupName = groupName };
            return false;
        }

        public static bool TrySetState(TempVisualStateGroup group, string stateName, IControl control)
        {
            //If state is already set, we can move on
            //Assumes Pseudoclasses aren't modified outside of VSM
            if (group.ActiveState != null && group.ActiveState.Equals(stateName, StringComparison.OrdinalIgnoreCase))
                return true;

            try
            {
                if (group.ActiveState != null)
                    ((IPseudoClasses)control.Classes).Remove(group.ActiveState);
                group.ActiveState = stateName;
                if (stateName != null)
                    ((IPseudoClasses)control.Classes).Add(group.ActiveState);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    internal class TempVisualStateGroup
    {
        public string GroupName { get; set; }
        public string ActiveState { get; internal set; }
    }
}
