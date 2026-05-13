using SIS.Common.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.Common.Pagination
{
    public class PagedRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = AppConstants.DefaultPageSize;
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
