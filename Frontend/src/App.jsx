import React from 'react'
import Login from './components/Login'
import Signup from './components/Signup'
import { Route, Routes } from 'react-router-dom'

const App = () => {
  return (
    <div>
      <Routes>
        <Route path='/' element={<Signup/>}></Route>
        <Route path='/login' element={<Login/>}></Route>
      </Routes>

      
    </div>
  )
}

export default App
