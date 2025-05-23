using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace MIDIFlux.GUI.Helpers
{
    /// <summary>
    /// Helper class for tracking dirty state of controls
    /// </summary>
    public class DirtyStateTracker
    {
        private readonly Control _parentControl;
        private readonly Dictionary<Control, object> _originalValues = new();
        private readonly List<Control> _trackedControls = new();
        private bool _isDirty = false;
        private bool _isTracking = false;

        /// <summary>
        /// Event raised when the dirty state changes
        /// </summary>
        public event EventHandler<bool>? DirtyStateChanged;

        /// <summary>
        /// Gets a value indicating whether the tracked controls have unsaved changes
        /// </summary>
        public bool IsDirty => _isDirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirtyStateTracker"/> class
        /// </summary>
        /// <param name="parentControl">The parent control containing the controls to track</param>
        public DirtyStateTracker(Control parentControl)
        {
            _parentControl = parentControl;
        }

        /// <summary>
        /// Starts tracking changes to the specified controls
        /// </summary>
        /// <param name="controls">The controls to track</param>
        public void StartTracking(params Control[] controls)
        {
            if (_isTracking)
            {
                StopTracking();
            }

            _isTracking = true;
            _isDirty = false;
            _originalValues.Clear();
            _trackedControls.Clear();

            foreach (var control in controls)
            {
                TrackControl(control);
            }
        }

        /// <summary>
        /// Starts tracking changes to all controls in the parent control
        /// </summary>
        public void StartTrackingAll()
        {
            if (_isTracking)
            {
                StopTracking();
            }

            _isTracking = true;
            _isDirty = false;
            _originalValues.Clear();
            _trackedControls.Clear();

            TrackControlsRecursive(_parentControl);
        }

        /// <summary>
        /// Stops tracking changes
        /// </summary>
        public void StopTracking()
        {
            foreach (var control in _trackedControls)
            {
                UntrackControl(control);
            }

            _isTracking = false;
            _isDirty = false;
            _originalValues.Clear();
            _trackedControls.Clear();
        }

        /// <summary>
        /// Resets the dirty state and updates the original values
        /// </summary>
        public void ResetDirtyState()
        {
            _originalValues.Clear();
            
            foreach (var control in _trackedControls)
            {
                _originalValues[control] = GetControlValue(control);
            }
            
            SetDirtyState(false);
        }

        /// <summary>
        /// Tracks changes to a control
        /// </summary>
        /// <param name="control">The control to track</param>
        private void TrackControl(Control control)
        {
            if (control is TextBox textBox)
            {
                _originalValues[textBox] = textBox.Text;
                textBox.TextChanged += Control_ValueChanged;
                _trackedControls.Add(textBox);
            }
            else if (control is CheckBox checkBox)
            {
                _originalValues[checkBox] = checkBox.Checked;
                checkBox.CheckedChanged += Control_ValueChanged;
                _trackedControls.Add(checkBox);
            }
            else if (control is RadioButton radioButton)
            {
                _originalValues[radioButton] = radioButton.Checked;
                radioButton.CheckedChanged += Control_ValueChanged;
                _trackedControls.Add(radioButton);
            }
            else if (control is ComboBox comboBox)
            {
                _originalValues[comboBox] = comboBox.SelectedIndex;
                comboBox.SelectedIndexChanged += Control_ValueChanged;
                _trackedControls.Add(comboBox);
            }
            else if (control is NumericUpDown numericUpDown)
            {
                _originalValues[numericUpDown] = numericUpDown.Value;
                numericUpDown.ValueChanged += Control_ValueChanged;
                _trackedControls.Add(numericUpDown);
            }
            else if (control is DateTimePicker dateTimePicker)
            {
                _originalValues[dateTimePicker] = dateTimePicker.Value;
                dateTimePicker.ValueChanged += Control_ValueChanged;
                _trackedControls.Add(dateTimePicker);
            }
            else if (control is TrackBar trackBar)
            {
                _originalValues[trackBar] = trackBar.Value;
                trackBar.ValueChanged += Control_ValueChanged;
                _trackedControls.Add(trackBar);
            }
            else if (control is ListBox listBox)
            {
                _originalValues[listBox] = listBox.SelectedIndex;
                listBox.SelectedIndexChanged += Control_ValueChanged;
                _trackedControls.Add(listBox);
            }
        }

        /// <summary>
        /// Tracks changes to all controls in a parent control
        /// </summary>
        /// <param name="parentControl">The parent control</param>
        private void TrackControlsRecursive(Control parentControl)
        {
            foreach (Control control in parentControl.Controls)
            {
                TrackControl(control);
                TrackControlsRecursive(control);
            }
        }

        /// <summary>
        /// Untracks changes to a control
        /// </summary>
        /// <param name="control">The control to untrack</param>
        private void UntrackControl(Control control)
        {
            if (control is TextBox textBox)
            {
                textBox.TextChanged -= Control_ValueChanged;
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.CheckedChanged -= Control_ValueChanged;
            }
            else if (control is RadioButton radioButton)
            {
                radioButton.CheckedChanged -= Control_ValueChanged;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.SelectedIndexChanged -= Control_ValueChanged;
            }
            else if (control is NumericUpDown numericUpDown)
            {
                numericUpDown.ValueChanged -= Control_ValueChanged;
            }
            else if (control is DateTimePicker dateTimePicker)
            {
                dateTimePicker.ValueChanged -= Control_ValueChanged;
            }
            else if (control is TrackBar trackBar)
            {
                trackBar.ValueChanged -= Control_ValueChanged;
            }
            else if (control is ListBox listBox)
            {
                listBox.SelectedIndexChanged -= Control_ValueChanged;
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of a control
        /// </summary>
        private void Control_ValueChanged(object? sender, EventArgs e)
        {
            if (sender is Control control && _originalValues.TryGetValue(control, out var originalValue))
            {
                var currentValue = GetControlValue(control);
                
                if (!Equals(originalValue, currentValue))
                {
                    SetDirtyState(true);
                }
                else
                {
                    // Check if any other controls are dirty
                    CheckAllControlsForDirtyState();
                }
            }
        }

        /// <summary>
        /// Gets the value of a control
        /// </summary>
        /// <param name="control">The control to get the value from</param>
        /// <returns>The value of the control</returns>
        private static object GetControlValue(Control control)
        {
            return control switch
            {
                TextBox textBox => textBox.Text,
                CheckBox checkBox => checkBox.Checked,
                RadioButton radioButton => radioButton.Checked,
                ComboBox comboBox => comboBox.SelectedIndex,
                NumericUpDown numericUpDown => numericUpDown.Value,
                DateTimePicker dateTimePicker => dateTimePicker.Value,
                TrackBar trackBar => trackBar.Value,
                ListBox listBox => listBox.SelectedIndex,
                _ => throw new NotSupportedException($"Control type {control.GetType().Name} is not supported")
            };
        }

        /// <summary>
        /// Checks all controls for dirty state
        /// </summary>
        private void CheckAllControlsForDirtyState()
        {
            foreach (var control in _trackedControls)
            {
                if (_originalValues.TryGetValue(control, out var originalValue))
                {
                    var currentValue = GetControlValue(control);
                    
                    if (!Equals(originalValue, currentValue))
                    {
                        SetDirtyState(true);
                        return;
                    }
                }
            }
            
            SetDirtyState(false);
        }

        /// <summary>
        /// Sets the dirty state
        /// </summary>
        /// <param name="isDirty">The new dirty state</param>
        private void SetDirtyState(bool isDirty)
        {
            if (_isDirty != isDirty)
            {
                _isDirty = isDirty;
                DirtyStateChanged?.Invoke(this, _isDirty);
            }
        }
    }
}

