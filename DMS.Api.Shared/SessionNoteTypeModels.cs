using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS.Api.Shared
{
    // Request Model
    public class SessionNoteTypeRequest
    {
        public string NoteTypeName { get; set; } = "";           // Required
        public string NoteTypeCode { get; set; } = "";           // Required
        public string? Description { get; set; }
        public string? UnitOfMeasure { get; set; }
        public bool IsMandatory { get; set; } = false;
        public bool IsNumeric { get; set; } = true;
        public decimal? MinimumValue { get; set; }
        public decimal? MaximumValue { get; set; }
        public string? DefaultValue { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public string? Category { get; set; }                    // VitalSigns, LabResults, Treatment, Observations, Other
        public int CreatedBy { get; set; }                       // Required
    }

    // Response Model
    public class SessionNoteTypeResponse
    {
        public int NoteTypeID { get; set; }
        public string NoteTypeName { get; set; } = "";
        public string NoteTypeCode { get; set; } = "";
        public string? Description { get; set; }
        public string? UnitOfMeasure { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsNumeric { get; set; }
        public decimal? MinimumValue { get; set; }
        public decimal? MaximumValue { get; set; }
        public string? DefaultValue { get; set; }
        public int DisplayOrder { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
