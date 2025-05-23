using System;
using System.Collections.Generic;

namespace MIDIFlux.Core.Models
{
    /// <summary>
    /// Represents the result of a validation operation
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets the list of errors found during validation
        /// </summary>
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Gets the list of warnings found during validation
        /// </summary>
        public List<string> Warnings { get; } = new List<string>();

        /// <summary>
        /// Gets a value indicating whether the validation found any errors
        /// </summary>
        public bool HasErrors => Errors.Count > 0;

        /// <summary>
        /// Gets a value indicating whether the validation found any warnings
        /// </summary>
        public bool HasWarnings => Warnings.Count > 0;

        /// <summary>
        /// Gets a value indicating whether the validation is successful (no errors)
        /// </summary>
        public bool IsValid => !HasErrors;

        /// <summary>
        /// Adds an error to the validation result
        /// </summary>
        /// <param name="error">The error message</param>
        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                Errors.Add(error);
            }
        }

        /// <summary>
        /// Adds a warning to the validation result
        /// </summary>
        /// <param name="warning">The warning message</param>
        public void AddWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning))
            {
                Warnings.Add(warning);
            }
        }

        /// <summary>
        /// Merges another validation result into this one
        /// </summary>
        /// <param name="other">The validation result to merge</param>
        public void Merge(ValidationResult other)
        {
            if (other == null)
            {
                return;
            }

            Errors.AddRange(other.Errors);
            Warnings.AddRange(other.Warnings);
        }

        /// <summary>
        /// Creates a new validation result with the specified error
        /// </summary>
        /// <param name="error">The error message</param>
        /// <returns>A new validation result with the specified error</returns>
        public static ValidationResult WithError(string error)
        {
            var result = new ValidationResult();
            result.AddError(error);
            return result;
        }

        /// <summary>
        /// Creates a new validation result with the specified warning
        /// </summary>
        /// <param name="warning">The warning message</param>
        /// <returns>A new validation result with the specified warning</returns>
        public static ValidationResult WithWarning(string warning)
        {
            var result = new ValidationResult();
            result.AddWarning(warning);
            return result;
        }

        /// <summary>
        /// Creates a new successful validation result
        /// </summary>
        /// <returns>A new successful validation result</returns>
        public static ValidationResult Success()
        {
            return new ValidationResult();
        }
    }
}
