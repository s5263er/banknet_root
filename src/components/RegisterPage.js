import React, { useState } from 'react';
import './RegisterPage.css';

function RegisterPage() {
  const [formData, setFormData] = useState({
    Email: '',
    Password: '',
    Name:  '',
  });

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const response = await fetch('http://localhost:5062/api/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
      });
      console.log(formData)
      
      if (response.ok) {
        // Registration successful
        alert('Registration successful!');
      } else {
        // Handle registration error
        const data = await response.json();
        console.error('Registration failed. Status:', response.status);
        console.error('Error message:', data.message);
        alert(`Registration failed: ${data.message}`);
      }
      
    } catch (error) {
      console.error('Error registering user:', error);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  return (
    <div className="register-page">
      <div className="login-container">
        <div className="blinking-text">
          <h1 className="splash-text">Layermark Bank</h1>
          <p className="splash-subtext">Future of Banking</p>
        </div>
        <form onSubmit={handleSubmit}>
         
          <input
            type="text"
            name="Name"
            placeholder="Full Name"
            className="your-css-class"
            value={formData.Name}
            onChange={handleChange}
          />
           <input
            type="email"
            name="Email"
            placeholder="Email"
            className="your-css-class"
            value={formData.Email}
            onChange={handleChange}
          />
          <input
            type="password"
            name="Password"
            placeholder="Password"
            className="your-css-class"
            value={formData.Password}
            onChange={handleChange}
          />
          <button type="submit" className="your-css-class">
            Register
          </button>
        </form>
        <p>Already have an account? <a href="/">Login</a></p>
      </div>
    </div>
  );
}

export default RegisterPage;
