import { useAuth } from "@/auth";
import { enums } from "chikexams-client";
import { useEffect } from "react";
import { useNavigate } from "react-router";

export const Home = () => {
  const { profile } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    // redirect to 
    const roles = profile?.roles ?? [];
      if (roles.includes(enums.UserRole.Admin)) {
        navigate('/users');
      } else if (roles.includes(enums.UserRole.Teacher)) {
        navigate('/quizzes');
      } else if (roles.includes(enums.UserRole.Student)) {
        navigate('/my-exams');
      } else {
        navigate('/');
      }
  }, [profile]);

    return <></>;
}