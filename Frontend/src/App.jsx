import React from 'react'
import Login from './Auth/Login'
import Signup from './Auth/Signup'
import { Route, Routes } from 'react-router-dom'
import Admin from './Dashboards/Admin'
import Customer from './Dashboards/Customer'

const App = () => {
  return (
    <div>
      <Routes>
        <Route path='/' element={<Signup />}></Route>
        <Route path='/login' element={<Login />}></Route>

        <Route path='/admin' element={<Admin />}></Route>
        <Route path='/customer' element={<Customer />}></Route>

      </Routes>


    </div>
  )
}

export default App
