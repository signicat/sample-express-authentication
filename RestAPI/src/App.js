import React, {Fragment, useState, useEffect} from "react";
import "./App.css";

const AuthDisplay = () => (
  <section>
    <h1> Welcome to the authentication process </h1>
    <div className="auth">
      <img src="./images/auth.png" alt="auth" />
      <form action="/authentication-session" method="POST">
        <button type="submit">
          Sign in
        </button>
      </form>
    </div>
  </section>
);

const Message = ({message}) => (
  <section>
    {message.name ?
      <Fragment>
        <h2>Successfully signed in!</h2>
        <p>Name: {message.name}</p>
        <p>NIN: {message.nin}</p>
      </Fragment>
      : <p>The sign in process was aborted or has an error."</p>}
  </section>
);

function App() {
  const [message, setMessage] = useState(null);

  useEffect(() => {
    const query = new URLSearchParams(window.location.search);

    if (query.get("success")) {
      setMessage({name: query.get("name"), nin: query.get("nin")});
    }
    if (query.get("canceled")) {
      setMessage({});
    }
    if (query.get("error")) {
      setMessage({});
    }
  }, []);

  return message ? (
    <Message message={message} />
  ) : (
    <AuthDisplay />
  );
}

export default App;
