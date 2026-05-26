import { Container } from "@/components/Container";
import { Button } from "@/components/Button";
import { FcGoogle } from "react-icons/fc";
import { FaGithub } from "react-icons/fa";

export default function Login() {
  return (
    <div className="flex flex-1 items-center justify-center px-4">
      <Container>
        <h2 className="max-w-2xl text-xl leading-tight font-black md:text-2xl">Welcome back</h2>
        <span className="text-text-secondary">Enter your details to access your account.</span>

        <div className="flex flex-col gap-1">
          <Button variant="outline" className="flex items-center justify-center gap-1.5">
            <FcGoogle size={20} />
            Continue with Google
          </Button>
          <Button variant="outline" className="flex items-center justify-center gap-1.5">
            <FaGithub size={20} />
            Continue with GitHub
          </Button>
        </div>
      </Container>
    </div>
  );
}
