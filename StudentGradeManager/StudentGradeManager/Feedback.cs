using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentGradeManager
{
    public class Feedback
    {
        public int UserId { get; set; }
        public string PraiseOrRemark { get; set; }
        public string Info { get; set; }
        public string AddedBy { get; set; }
        public Feedback(string praiseOrRemark, string info, string addedBy)
        {
            if (string.IsNullOrWhiteSpace(praiseOrRemark) ||
                (praiseOrRemark.ToLower() != "praise" && praiseOrRemark.ToLower() != "remark"))
                throw new ArgumentException("PraiseOrRemark must be either 'praise' or 'remark'.", nameof(praiseOrRemark));

            if (string.IsNullOrWhiteSpace(info))
                throw new ArgumentException("Info cannot be null or empty.", nameof(info));
            if (info.Length > 500)
                throw new ArgumentException("Info cannot exceed 500 characters.", nameof(info));

            if (string.IsNullOrWhiteSpace(addedBy))
                throw new ArgumentException("AddedBy cannot be null or empty.", nameof(addedBy));
            if (addedBy.Length > 100)
                throw new ArgumentException("AddedBy cannot exceed 100 characters.", nameof(addedBy));

            PraiseOrRemark = praiseOrRemark;
            Info = info;
            AddedBy = addedBy;
        }
        public override string ToString()
        {
            return $"StudentId: {UserId}, Type: {PraiseOrRemark}, Info: \"{Info}\", Added By: {AddedBy}";
        }
        public void DetailedInfo(Database db)
        {
            var student = db.GetStudentByID(this.UserId);
            if (student != null)
            {
                Console.WriteLine($"{student.Username} ({student.FullName}), Type: {PraiseOrRemark}, Info: \"{Info}\", Added By: {AddedBy}");
            }
            else
            {
                Console.WriteLine($"No student found with ID {this.UserId}.");
            }
        }
    }
}
