using System.Collections.Generic;
using CoursesApi.Models.DTOModels;
using CoursesApi.Models.ViewModels;

namespace CoursesApi.Repositories
{
    public interface ICoursesRepository
    {
        IEnumerable<CoursesListItemDTO> GetCourses(string semsester);
        CourseDetailsDTO GetCourseById(int courseId);
        CourseDetailsDTO AddCourse(CourseViewModel newCourse);
        CourseDetailsDTO UpdateCourse(int courseId, CourseViewModel updatedCourse);
        IEnumerable<StudentDTO> GetStudentsByCourseId(int courseId);

        StudentDTO GetStudentBySSn(string ssn);
        StudentDTO AddStudentToCourse(int courseId, StudentViewModel newStudent);
        
        StudentDTO AddStudentToWaitingList(int courseId, StudentViewModel newStudent);
        bool IsStudentInCourse(string studentSSn, int courseId);
        bool IsStudentOnCrsWL(string studentSSn, int courseId);

        bool RemoveStudentFromWaitingList(string studentSSn, int courseId);

        bool DeleteCourseById(int courseId);
        bool RemoveStudentFromCourse(string studentSSn, int courseId);
    }
}


