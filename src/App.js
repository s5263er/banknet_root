import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';

import LoginPage from './components/LoginPage';
import RegisterPage from './components/RegisterPage';
import HomePage from './components/HomePage';
import StocksPage from './components/StocksPage';

function App() {
  return (
    <Router>
      <div className="App">
        <Routes>
          <Route path="/" element={<LoginPage />} />
          <Route path="/home" element={<HomePage />} />
          <Route path="/signup" element={<RegisterPage />} />
          <Route path="/stocks" element={<StocksPage />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
