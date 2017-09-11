using System.Collections.Generic;
using CoursesApi.Models.DTOModels;
using CoursesApi.Repositories;
using System;
using CoursesApi.Models.ViewModels;

namespace CoursesApi.Services
{
    public class CoursesService : ICoursesService
    {
        private ICoursesRepository _repo;

        public StudentDTO AddStudentToWaitList(int courseId, StudentViewModel newStudent){
            //student must be in system.
            StudentDTO stu = _repo.GetStudentBySSn(newStudent.SSN);
            if (stu == null){ return null; }
            //course must exist.s
            CourseDetailsDTO crs = _repo.GetCourseById(courseId);
            if(crs == null) { return null; }
            //if student is on course already. do not add him to waiting list;
            if(_repo.IsStudentInCourse(newStudent.SSN, courseId)){
                return null;
            }

            //if student is on list already.
            if(_repo.IsStudentOnCrsWL(newStudent.SSN, courseId)){
                return null;
            }

            return _repo.AddStudentToWaitingList(courseId, newStudent);


        }


        public CoursesService(ICoursesRepository repo)
        {
            _repo = repo;
        }
        
        public IEnumerable<CoursesListItemDTO> GetCourses(string semester)
        {            
            var courses = _repo.GetCourses(semester);
            
            return courses;
        }

        public CourseDetailsDTO GetCourseById(int courseId)
        {
            var course = _repo.GetCourseById(courseId);
        
            return course;
        }

        public CourseDetailsDTO UpdateCourse(int courseId, CourseViewModel updatedCourse)
        {
            var course = _repo.UpdateCourse(courseId, updatedCourse);

            return course;
        }

        public IEnumerable<StudentDTO> GetStudentsByCourseId(int courseId)
        {
            var students = _repo.GetStudentsByCourseId(courseId);

            return students;
        }


        public StudentDTO AddStudentToCourse(int courseId,  StudentViewModel newStudent)
        {

            //check if student exists.
            StudentDTO student = _repo.GetStudentBySSn(newStudent.SSN);
            if(student == null){
                return null;
            }

            //is student already in course?
            if(_repo.IsStudentInCourse(newStudent.SSN, courseId)){
                return null;
            }

            //make sure that course exist.
            CourseDetailsDTO course = _repo.GetCourseById(courseId);
            if(course == null){
                return null;
            }

            //do not allow more students than maxstudenst
            if(course.MaxStudents <= course.Students.Count){
                return null;
            }

            //all is good. check if he is on waitng list and if so remove him.
            //Todo: does this need to be 2 different functions...?
            if( _repo.IsStudentOnCrsWL(newStudent.SSN, courseId) ){
                _repo.RemoveStudentFromWaitingList(newStudent.SSN, courseId);
            }

            var stu = _repo.AddStudentToCourse(courseId, newStudent);
            
            return stu;
        }


        public bool DeleteCourseById(int courseId)
        {
            var result = _repo.GetCourseById(courseId);
            if(result == null){
                return false;
            }

            if(result.Students.Count > 0){
                foreach(StudentDTO stu in result.Students){
                    _repo.RemoveStudentFromCourse(stu.SSN, courseId);
                }
            }

            var res = _repo.DeleteCourseById(courseId);

            return res;
        }

        public CourseDetailsDTO AddCourse(CourseViewModel newCourse)
        {
            return _repo.AddCourse(newCourse);
        }
    }
}