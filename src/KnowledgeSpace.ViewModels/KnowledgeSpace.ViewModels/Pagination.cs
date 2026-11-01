using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnowledgeSpace.ViewModels
{
    public class Pagination<T>
    {
        public List<T> Items { get; set; }
        public int TotalRecords { get; set; }
     }
}
