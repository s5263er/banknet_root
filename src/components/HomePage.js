import React, { useState, useEffect } from 'react';
import axios from 'axios';
import transfer_logo from "./assets/transfer.png";
import atm_logo from "./assets/atm.png";
import stocks_logo from "./assets/stocks.png";
import loan_logo from "./assets/loan.png";
import { Link } from 'react-router-dom';




function HomePage() {
  const [balance, setBalance] = useState(null);

  useEffect(() => {
    // Retrieve JWT 
    const token = localStorage.getItem('jwtToken');
    console.log(token);

    if (token) {
      axios
        .get('http://localhost:5062/api/balance', {
          // Include JWT token in header
          headers: {
            Authorization: `Bearer ${token}`,
          },
        })
        .then((response) => {
          console.log('Response from server:', response);
          setBalance(response.data.balance);
        })
        .catch((error) => {
          console.error('Error fetching balance', error);
        });
    } else {
      console.error('JWT token not found');
    }
  }, []);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'flex-start', height: '100vh', background: 'linear-gradient(135deg, #000, #111)', color: '#fff', fontFamily: 'Montserrat, sans-serif' }}>
     <div style={{ width: '100%', padding: '20px 0', textAlign: 'center', backgroundColor: '#333' }} className="app-bar">
  <div style={{ fontSize: '24px', fontWeight: 'bold' }} className="app-title">Layermark Bank</div>
</div>


      <div style={{ marginTop: '40px', backgroundColor: '#333', borderRadius: '12px', padding: '20px', boxShadow: '0 8px 20px rgba(0, 0, 0, 0.6)', display: 'flex', flexDirection: 'column', alignItems: 'center' }} className="balance-widget">
        <h2 style={{ fontSize: '36px', marginBottom: '10px' }}>Your Balance</h2>
        {/* Your futuristic balance display can go here */}
        <div className="futuristic-balance"> <h2>${balance}</h2></div>
      </div>

      <div style={{ display: 'flex', justifyContent: 'center', gap: '20px', marginTop: '20px' }} className="squares-container">
        <div style={{ width: '200px', height: '200px', backgroundColor: '#fff', color: '#000', borderRadius: '12px', padding: '20px', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', transition: 'transform 0.3s, background-color 0.3s' }} className="square payments">
          <img src={transfer_logo} alt="Payments" style={{ width: '60px', height: '60px', marginBottom: '10px' }} />
          <p>Payments</p>
        </div>
        <div style={{ width: '200px', height: '200px', backgroundColor: '#fff', color: '#000', borderRadius: '12px', padding: '20px', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', transition: 'transform 0.3s, background-color 0.3s' }} className="square atm">
          <img src={atm_logo} alt="ATM" style={{ width: '60px', height: '60px', marginBottom: '10px' }} />
          <p>ATM</p>
        </div>
        <div style={{ width: '200px', height: '200px', backgroundColor: '#fff', color: '#000', borderRadius: '12px', padding: '20px', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', transition: 'transform 0.3s, background-color 0.3s' }} className="square stocks">
        <Link to="/stocks"> 
          <img src={stocks_logo} alt="Stocks" style={{ width: '60px', height: '60px', marginBottom: '10px' }} />
          <p>Stocks</p>
          </Link>
        </div>
        <div style={{ width: '200px', height: '200px', backgroundColor: '#fff', color: '#000', borderRadius: '12px', padding: '20px', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', transition: 'transform 0.3s, background-color 0.3s' }} className="square loans">
          <img src={loan_logo} alt="Loans" style={{ width: '60px', height: '60px', marginBottom: '10px' }} />
          <p>Loans</p>
        </div>
      </div>

      <div style={{ marginTop: '20px', padding: '20px', backgroundColor: '#111', color: '#fff', textAlign: 'center', borderTopLeftRadius: '12px', borderTopRightRadius: '12px' }}>
        <p>This website is for demonstration purposes only. The data and information provided on this site do not constitute financial advice. Layermark Bank is a fictional entity, and any resemblance to real banks is purely coincidental.</p>
        <p>Powered by Erdem Yildiz</p>
      </div>
    </div>
  );
}

export default HomePage;
