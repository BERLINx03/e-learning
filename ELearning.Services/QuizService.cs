using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ELearning.Data.Models;
using ELearning.Repositories.Interfaces;
using ELearning.Services.Interfaces;
using ELearning.Services.Dtos;

namespace ELearning.Services
{
    public class QuizService : IQuizService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IRepository<QuizQuestion> _questionRepository;
        private readonly IRepository<QuizAnswer> _answerRepository;

        public QuizService(
            ILessonRepository lessonRepository,
            IRepository<QuizQuestion> questionRepository,
            IRepository<QuizAnswer> answerRepository)
        {
            _lessonRepository = lessonRepository;
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
        }

        public async Task<QuizQuestion> CreateQuizQuestionAsync(int lessonId, QuizQuestionCreateDto questionDto)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                throw new ArgumentException("Lesson not found");

            if (!lesson.IsQuiz)
                throw new InvalidOperationException("This lesson is not marked as a quiz");

            var question = new QuizQuestion
            {
                LessonId = lessonId,
                QuestionText = questionDto.QuestionText,
                Points = questionDto.Points,
                Answers = new List<QuizAnswer>()
            };

            await _questionRepository.AddAsync(question);
            await _questionRepository.SaveChangesAsync();

            if (questionDto.Answers != null && questionDto.Answers.Any())
            {
                foreach (var answerDto in questionDto.Answers)
                {
                    await CreateQuizAnswerAsync(question.Id, answerDto);
                }
            }

            return question;
        }

        public async Task<QuizAnswer> CreateQuizAnswerAsync(int questionId, QuizAnswerCreateDto answerDto)
        {
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question == null)
                throw new ArgumentException("Question not found");

            var answer = new QuizAnswer
            {
                QuestionId = questionId,
                AnswerText = answerDto.AnswerText,
                IsCorrect = answerDto.IsCorrect
            };

            await _answerRepository.AddAsync(answer);
            await _answerRepository.SaveChangesAsync();

            return answer;
        }

        public async Task<IEnumerable<QuizQuestionDto>> GetQuizQuestionsByLessonIdAsync(int lessonId)
        {
            var questions = await _lessonRepository.GetQuizQuestionsAsync(lessonId);

            return questions.Select(q => new QuizQuestionDto
            {
                Id = q.Id,
                LessonId = q.LessonId,
                QuestionText = q.QuestionText,
                Points = q.Points,
                Answers = q.Answers?.Select(a => new QuizAnswerDto
                {
                    Id = a.Id,
                    QuestionId = a.QuestionId,
                    AnswerText = a.AnswerText,
                    IsCorrect = a.IsCorrect
                }).ToList()
            }).ToList();
        }

        public async Task<bool> UpdateQuizQuestionAsync(int questionId, QuizQuestionCreateDto questionDto)
        {
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question == null)
                return false;

            question.QuestionText = questionDto.QuestionText;
            question.Points = questionDto.Points;

            _questionRepository.Update(question);
            await _questionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateQuizAnswerAsync(int answerId, QuizAnswerCreateDto answerDto)
        {
            var answer = await _answerRepository.GetByIdAsync(answerId);
            if (answer == null)
                return false;

            answer.AnswerText = answerDto.AnswerText;
            answer.IsCorrect = answerDto.IsCorrect;

            _answerRepository.Update(answer);
            await _answerRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuizQuestionAsync(int questionId)
        {
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question == null)
                return false;

            _questionRepository.Remove(question);
            await _questionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQuizAnswerAsync(int answerId)
        {
            var answer = await _answerRepository.GetByIdAsync(answerId);
            if (answer == null)
                return false;

            _answerRepository.Remove(answer);
            await _answerRepository.SaveChangesAsync();
            return true;
        }

        public async Task<int> CalculateQuizScoreAsync(int lessonId, Dictionary<int, int> userAnswers)
        {
            return await _lessonRepository.CalculateQuizScoreAsync(lessonId, userAnswers);
        }
    }
}