using System;
using System.Linq;
using System.Collections.Generic;
using CoursesApi.Models.DTOModels;
using CoursesApi.Models.EntityModels;
using AutoMapper;
using CoursesApi.Models.ViewModels;

namespace CoursesApi.Repositories
{
    public class CoursesRepository : ICoursesRepository
    {
        private AppDataContext _db;
        
        public CoursesRepository(AppDataContext db)
        {
            _db = db;
        }

        public IEnumerable<CoursesListItemDTO> GetCourses(string semsester)
        {
            // TESTEER
            _db.Students.Add(new Student{ SSN="1234567890", Name="Herp McDerpsson 1"});
            _db.Students.Add(new Student{ SSN="1234567891", Name="Herpina Derpy 1"});
            _db.Students.Add(new Student{ SSN="1234567892", Name="Herp McDerpsson 2"});
            _db.Students.Add(new Student{ SSN="1234567893", Name="Herpina Derpy 2"});
            _db.Students.Add(new Student{ SSN="1234567894", Name="Herp McDerpsson 3"});
            _db.Students.Add(new Student{ SSN="1234567895", Name="Herpina Derpy 3"});
            _db.Students.Add(new Student{ SSN="1234567896", Name="Herp McDerpsson 4"});
            _db.Students.Add(new Student{ SSN="1234567897", Name="Herpina Derpy 4"});
            _db.Students.Add(new Student{ SSN="1234567898", Name="Herp McDerpsson 5"});
            _db.Students.Add(new Student{ SSN="1234567899", Name="Herpina Derpy 5"});

            _db.CourseTemplates.Add(new CourseTemplate{Template="T-514-VEFT", CourseName="Vefthjonustur"});
            _db.SaveChanges();

            // TEEEEESTEEEER
            var courses = (from c in _db.Courses
                           join t in _db.CourseTemplates on c.CourseTemplate equals t.Template 
                           where c.Semester == semsester
                           select new CoursesListItemDTO 
                           {
                               Id = c.Id,
                               Name = t.CourseName,
                               NumberOfStudents = (_db.Enrollments.Count(s => s.CourseId == c.Id))
                           }).ToList();

            return courses;
        }

        public CourseDetailsDTO GetCourseById(int courseId)
        {            
            var course = _db.Courses.SingleOrDefault(c => c.Id == courseId);

            if (course == null) 
            {
                return null;
            }

            var result = new CourseDetailsDTO
            {
                Id = course.Id,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Name = _db.CourseTemplates.Where(t => t.Template == course.CourseTemplate)
                                                         .Select(c => c.CourseName).FirstOrDefault(),
                Students = (from sr in _db.Enrollments
                           where sr.CourseId == course.Id
                           join s in _db.Students on sr.StudentSSN equals s.SSN
                           select new StudentDTO
                           {
                               SSN = s.SSN,
                               Name = s.Name
                           }).ToList(),

                StudentsInWaitingList = (from sr in _db.WaitingListRelations
                                         where sr.CourseId == course.Id
                                         join s in _db.Students on sr.StudentSSN equals s.SSN
                                         select new StudentDTO{
                                            SSN = s.SSN,
                                            Name = s.Name
                                         }).ToList()
            };

            return result;

        }

        public bool IsStudentInCourse(string SSn, int courseId){
            var floo = (from rel in _db.Enrollments
                        where rel.StudentSSN == SSn &&
                        rel.CourseId == courseId
                        select rel).SingleOrDefault();
            
            return (floo != null);
        }

        public CourseDetailsDTO UpdateCourse(int courseId, CourseViewModel updatedCourse)
        {
            var course = _db.Courses.SingleOrDefault(c => c.Id == courseId);

            if (course == null) 
            {
                return null;
            }

            course.StartDate = updatedCourse.StartDate;
            course.EndDate = updatedCourse.EndDate;

            _db.SaveChanges();

            return GetCourseById(courseId);
        }

        public IEnumerable<StudentDTO> GetStudentsByCourseId(int courseId)
        {
            var course = _db.Courses.SingleOrDefault(c => c.Id == courseId);

            if (course == null) 
            {
                return null;
            }

            var students = (from sr in _db.Enrollments
                            where sr.CourseId == courseId
                            join s in _db.Students on sr.StudentSSN equals s.SSN
                            select new StudentDTO
                            {
                                SSN = s.SSN,
                                Name = s.Name
                            }).ToList();

            return students;
        }



        public StudentDTO AddStudentToCourse(int courseId, StudentViewModel newStudent)
        {
            var course = (from c in _db.Courses
                          where c.Id == courseId
                          select c).SingleOrDefault();

            var student = GetStudentBySSn(newStudent.SSN);
            if (course == null || student == null)
            {
                return null;
            }

            
            //do not permit an existing enrollment to be repeated.
            if(GetEnrollment(student.SSN, course.Id) != null){
                return null;
            }

            _db.Enrollments.Add( 
                new Enrollment {CourseId = courseId, StudentSSN = newStudent.SSN}
            );
            _db.SaveChanges();

            return new StudentDTO
            {
                SSN = newStudent.SSN,
                Name = (from st in _db.Students
                       where st.SSN == newStudent.SSN
                       select st).SingleOrDefault().Name
            };
        }

        private WaitingListRelation getWaitlistRelation(string SSn, int CourseId){
            var relation = (from rel in _db.WaitingListRelations        
                where rel.StudentSSN == SSn &&
                rel.CourseId == CourseId
                select rel).SingleOrDefault();
            return relation;

        }

        public StudentDTO AddStudentToWaitingList(int courseId, StudentViewModel newStudent){
            
            //check if student is in system.
            StudentDTO stu = GetStudentBySSn(newStudent.SSN);
            if(stu == null){return null; }

            WaitingListRelation w = getWaitlistRelation(newStudent.SSN, courseId);
            if(w != null){ return null; }
            
            WaitingListRelation wlr = new WaitingListRelation{
                StudentSSN = newStudent.SSN,
                CourseId = courseId
            };
            _db.WaitingListRelations.Add(wlr);
            _db.SaveChanges();

            return new StudentDTO{
                Name = GetStudentBySSn(newStudent.SSN).Name,// what the fuck
                SSN = newStudent.SSN
            };
        }
        public bool RemoveStudentFromWaitingList(string SSn, int courseId){
            
            WaitingListRelation relation = getWaitlistRelation(SSn, courseId);

            if(relation == null){
                return false;
            }

            _db.WaitingListRelations.Remove(relation);
            _db.SaveChanges();
            return true;

        }

        public bool IsStudentOnCrsWL(string ssn, int courseId){
           
         
            return (getWaitlistRelation(ssn, courseId) != null);

        }

        public StudentDTO GetStudentBySSn(string ssn){

            var res = (from stu in _db.Students
                        where stu.SSN == ssn
                        select new StudentDTO{
                            Name = stu.Name,
                            SSN = stu.SSN
                        }).SingleOrDefault();

            return res;

        }

        public Enrollment GetEnrollment(string ssn, int courseId){
            var enrollment = (from e in _db.Enrollments
                              where e.StudentSSN == ssn &&
                              e.CourseId == courseId
                              select e).Single();
            if(enrollment == null){return null; }
            return enrollment;
        }

        public bool DeleteCourseById(int courseId)
        {
            var course = (from c in _db.Courses
                            where c.Id == courseId
                            select c).SingleOrDefault();
            
            if (course == null)
            {
                return false;
            }
            
            
            _db.Courses.Remove(course);
            _db.SaveChanges();

            return true;
        }
        

        public bool RemoveStudentFromCourse(string studentSSn, int courseId){

            var relation = (from rel in _db.Enrollments
                            where rel.StudentSSN == studentSSn &&
                            rel.CourseId == courseId
                            select rel).SingleOrDefault();
            
            if(relation == null){
                return false;
            }

            _db.Enrollments.Remove(relation);
            _db.SaveChanges();
            return true;


        }

        /// <summary>
        /// Gets a single student by ssn, throws if there are more than one
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public StudentDTO GetStudentBySsn(string ssn){

            var student = (from s in _db.Students
                            where s.SSN == ssn
                            select s).Single();

            if(student == null)
            {
                return null;
            }

            return new StudentDTO{
                SSN=student.SSN,
                Name=student.Name
            };

        }
        /* 
        public bool IsStudentInCourse(string SSn, int courseId){
            var res = ( from rel in _db.Enrollments
                        where rel.StudentSSN == SSn &&
                        rel.CourseId == courseId 
                        select rel).SingleOrDefault();
            return (res != null);
        }
        */

        public CourseDetailsDTO AddCourse(CourseViewModel newCourse)
        {
            var entity = new Course { 
                CourseTemplate = newCourse.CourseID,
                Semester = newCourse.Semester,
                StartDate = newCourse.StartDate,
                EndDate = newCourse.EndDate,
                MaxStudents = newCourse.MaxStudents
            };

            _db.Courses.Add(entity);
            _db.SaveChanges();

            return new CourseDetailsDTO 
            {
                Id = entity.Id,
                Name = _db.CourseTemplates.FirstOrDefault(ct => ct.Template == newCourse.CourseID).CourseName,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Students = _db.Enrollments
                    .Where(e => e.CourseId == entity.Id)
                    .Join(_db.Students, enroll => enroll.StudentSSN, stud => stud.SSN, (e, s) => s)
                    .Select(s => new StudentDTO {
                        SSN = s.SSN,
                        Name = s.Name
                    }).ToList()
            };
        }
    }
} 
