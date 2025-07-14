using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamProject.Domain;

public class DishViewModel
{
    public string DishName { get; set; }
    public string CategoryName { get; set; }
    public string PriceFormatted { get; set; }
}
