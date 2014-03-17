using System;
using Iesi.Collections.Generic;

namespace Diagnosis.Models
{
    public class Patient
    {
        public virtual int Id { get; protected set; }
        public virtual string FirstName { get; set; }
        public virtual string MiddleName { get; set; }
        public virtual string LastName { get; set; }
        public virtual bool IsMale { get; set; }
        public virtual DateTime BirthDate { get; set; }
        public virtual string SNILS
        {
            get
            {
                return _snils;
            }
            set
            {
                if (_snils != value && CheckSnils(value))
                {
                    _snils = value;
                }
            }
        }

        static bool CheckSnils(string snils)
        {
            if (snils.Length != 11)
                return false;

            int number;
            int control;
            if (!int.TryParse(snils.Substring(0, 9), out number) ||
                !int.TryParse(snils.Substring(9, 2), out control))
                return false;

            if (number <= 1001998)
                return true;

            int sum = 0;
            for (int i = 1; i <= 9; i++)
            {
                sum += (number % 10) * i;
                number /= 10;
            }
            if (sum < 100)
            {
                return sum == control;
            }
            if (sum > 101)
            {
                return sum == control % 101;
            }
            return 0 == control;
        }


        public Patient()
        {
            BirthDate = new DateTime(1980, 6, 15);
            IsMale = true;
        }
    }
}
