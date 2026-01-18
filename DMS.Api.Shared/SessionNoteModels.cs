using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.Shared
{
    // Request Models

    /// <summary>
    /// Add session note
    /// </summary>
    public class AddSessionNoteRequest
    {
        public int SessionID { get; set; }                       // Required
        public int NoteTypeID { get; set; }                      // Required
        public string NoteValue { get; set; } = "";              // Required
        public string? AdditionalNotes { get; set; }
        public int RecordedBy { get; set; }                      // Required
    }

    /// <summary>
    /// Add multiple notes (batch)
    /// </summary>
    public class AddBulkSessionNotesRequest
    {
        public int SessionID { get; set; }                       // Required
        public List<SessionNoteInput> Notes { get; set; } = new();
        public int RecordedBy { get; set; }                      // Required
    }

    public class SessionNoteInput
    {
        public int NoteTypeID { get; set; }
        public string NoteValue { get; set; } = "";
        public string? AdditionalNotes { get; set; }
    }

    // Response Models

    /// <summary>
    /// Session note response
    /// </summary>
    public class SessionNoteResponse
    {
        public int SessionNoteID { get; set; }
        public int SessionID { get; set; }

        // Note Type Details
        public int NoteTypeID { get; set; }
        public string NoteTypeName { get; set; } = "";
        public string NoteTypeCode { get; set; } = "";
        public string? UnitOfMeasure { get; set; }
        public string? Category { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsNumeric { get; set; }
        public decimal? MinimumValue { get; set; }
        public decimal? MaximumValue { get; set; }

        // Note Details
        public string NoteValue { get; set; } = "";
        public DateTime NoteTime { get; set; }
        public bool IsAbnormal { get; set; }
        public bool AlertGenerated { get; set; }

        public int RecordedBy { get; set; }
        public string? Notes { get; set; }

        // Calculated Properties
        public string DisplayValue => !string.IsNullOrEmpty(UnitOfMeasure)
            ? $"{NoteValue} {UnitOfMeasure}"
            : NoteValue;

        public string StatusIndicator => IsAbnormal ? "⚠️" : "✓";
    }

    /// <summary>
    /// Grouped notes by category
    /// </summary>
    public class GroupedSessionNotesResponse
    {
        public string Category { get; set; } = "";
        public List<SessionNoteResponse> Notes { get; set; } = new();
    }

    /// <summary>
    /// Validation result for mandatory notes
    /// </summary>
    public class MandatoryNotesValidationResponse
    {
        public bool AllMandatoryNotesRecorded { get; set; }
        public List<SessionNoteTypeResponse> MissingMandatoryNotes { get; set; } = new();
        public string ValidationMessage { get; set; } = "";
    }
}
