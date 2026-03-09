import { Link, useParams } from 'react-router';
import { ioc } from '@/utils/ioc';
import * as chikexamsService from '@/services/chikexams.service';
import { useExam, useExamAnswers, useExamScores } from '@/services/chikexams.hooks';
import { ChevronLeft } from 'lucide-react';
import { CircularProgress } from '@mui/material';

const QUESTION_TYPE_LABELS: Record<number, string> = {
  1: 'Single Choice',
  2: 'Multiple Choice',
  3: 'True or False',
  4: 'Fill in the Blank',
  5: 'Short Answer',
  6: 'Essay',
};

const getScoreColor = (finalScore: number | null | undefined, maxScore: number): string => {
  if (finalScore == null) return '#6B7280';
  if (finalScore === maxScore) return '#10B981'; // green = full marks
  if (finalScore > 0) return '#F59E0B'; // yellow = partial
  return '#EF4444'; // red = zero
};

const getScoreBg = (finalScore: number | null | undefined, maxScore: number): string => {
  if (finalScore == null) return '#F3F4F6';
  if (finalScore === maxScore) return '#ECFDF5';
  if (finalScore > 0) return '#FFFBEB';
  return '#FEF2F2';
};

export const StudentExamReview = ({
  getExam: getExamFn = ioc((keys) => keys.getExam) || chikexamsService.getExam,
  getExamAnswers: getExamAnswersFn = ioc((keys) => keys.getExamAnswers) || chikexamsService.getExamAnswers,
  getExamScores: getExamScoresFn = ioc((keys) => keys.getExamScores) || chikexamsService.getExamScores,
}: {
  getExam?: typeof chikexamsService.getExam;
  getExamAnswers?: typeof chikexamsService.getExamAnswers;
  getExamScores?: typeof chikexamsService.getExamScores;
}) => {
  const { id } = useParams<{ id: string }>();
  const examId = parseInt(id!);

  const { data: exam, isLoading: examLoading } = useExam(examId, { getExam: getExamFn });
  const { data: answers, isLoading: answersLoading } = useExamAnswers(examId, { getExamAnswers: getExamAnswersFn });
  const { data: scores } = useExamScores(examId, { getExamScores: getExamScoresFn });

  if (examLoading) {
    return (
      <div className="flex items-center justify-center py-16">
        <CircularProgress />
      </div>
    );
  }

  if (!exam) {
    return <div className="text-center py-16 text-sm" style={{ color: '#6B7280' }}>Exam not found.</div>;
  }

  const getAnswerScore = (questionId: number) =>
    scores?.answerScores?.find((s) => s.questionId === questionId);

  return (
    <div className="max-w-3xl mx-auto">
      <div className="mb-4">
        <Link
          to="/my-exams"
          className="flex items-center gap-1 text-sm hover:underline"
          style={{ color: '#314CB6' }}
        >
          <ChevronLeft size={16} />
          Back to My Exams
        </Link>
      </div>

      <div className="bg-white rounded-xl shadow-sm p-6 mb-6" style={{ border: '1px solid #E5E7EB' }}>
        <h1 className="text-xl font-bold mb-2" style={{ color: '#211A1E' }}>
          {exam.quiz?.title ?? `Quiz ${exam.quizId}`} — Review
        </h1>
        <div className="flex flex-wrap gap-4 text-sm mb-4" style={{ color: '#6B7280' }}>
          {exam.endedAt && (
            <span>Submitted: <strong style={{ color: '#211A1E' }}>{new Date(exam.endedAt).toLocaleString()}</strong></span>
          )}
          {scores && (
            <span>Final Score: <strong style={{ color: '#211A1E' }}>{scores.totalScore} / {scores.maxPossibleScore}</strong></span>
          )}
        </div>

        {exam.examinerComment && (
          <div className="p-3 rounded-lg" style={{ backgroundColor: '#EBF2FA', border: '1px solid #314CB6' }}>
            <p className="text-sm font-medium mb-1" style={{ color: '#314CB6' }}>Examiner Comment</p>
            <p className="text-sm" style={{ color: '#211A1E' }}>{exam.examinerComment}</p>
          </div>
        )}
      </div>

      {answersLoading ? (
        <div className="flex items-center justify-center py-8">
          <CircularProgress size={24} />
        </div>
      ) : (
        <div className="flex flex-col gap-4">
          {(answers ?? []).map((answer, idx) => {
            const question = answer.question;
            const scoreInfo = getAnswerScore(answer.questionId);
            const finalScore = scoreInfo?.finalScore ?? null;
            const maxScore = scoreInfo?.maxScore ?? question?.score ?? 0;
            const color = getScoreColor(finalScore, maxScore);
            const bgColor = getScoreBg(finalScore, maxScore);

            return (
              <div
                key={answer.id}
                className="bg-white rounded-xl shadow-sm p-5"
                style={{ border: `1px solid ${color}` }}
              >
                <div className="flex items-start justify-between mb-2">
                  <div>
                    <span className="text-sm font-semibold" style={{ color: '#211A1E' }}>
                      Q{idx + 1}. {question?.prompt ?? `Question ${answer.questionId}`}
                    </span>
                    {question && (
                      <span
                        className="ml-2 text-xs px-2 py-0.5 rounded"
                        style={{ backgroundColor: '#F3F4F6', color: '#6B7280' }}
                      >
                        {QUESTION_TYPE_LABELS[question.typeId] ?? 'Unknown'}
                      </span>
                    )}
                  </div>
                  <span
                    className="text-sm font-bold px-2 py-0.5 rounded"
                    style={{ backgroundColor: bgColor, color }}
                  >
                    {finalScore != null ? finalScore : '—'} / {maxScore}
                    {finalScore === maxScore && maxScore > 0 ? ' ✓' : finalScore === 0 ? ' ✗' : ''}
                  </span>
                </div>

                <div className="mb-2">
                  <p className="text-xs font-medium mb-1" style={{ color: '#6B7280' }}>Your Answer:</p>
                  <p
                    className="text-sm p-2 rounded"
                    style={{
                      backgroundColor: bgColor,
                      color: '#211A1E',
                      border: `1px solid ${color}20`,
                    }}
                  >
                    {answer.answer ?? <em style={{ color: '#6B7280' }}>No answer provided</em>}
                  </p>
                </div>

                {answer.examinerScore != null && (
                  <p className="text-xs" style={{ color: '#6B7280' }}>
                    Examiner score: <strong style={{ color: '#211A1E' }}>{answer.examinerScore}</strong>
                    {answer.examinerComment && (
                      <> | <span style={{ color: '#211A1E' }}>&ldquo;{answer.examinerComment}&rdquo;</span></>
                    )}
                  </p>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};
