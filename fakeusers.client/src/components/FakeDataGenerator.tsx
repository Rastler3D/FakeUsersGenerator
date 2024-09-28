'use client'

import { useState, useEffect, useRef } from 'react'
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Slider } from "@/components/ui/slider"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { ScrollArea } from '@/components/ui/scroll-area'

type Region = 'USA' | 'Poland' | 'Ukraine'
type UserData = {
    number: number
    id: string
    fullName: string
    address: string
    phone: string
}

export default function FakeDataGenerator() {
    const [region, setRegion] = useState<Region>('USA')
    const [errorCount, setErrorCount] = useState(0)
    const [seed, setSeed] = useState('')
    const [userData, setUserData] = useState<UserData[]>([])
    const [page, setPage] = useState(0)
    const [loading, setLoading] = useState(false)
    const observerTarget = useRef(null)
    const [loadingError, setLoadingError] = useState(false)
    const [isChangingError, setIsChangingError] = useState(false)

    useEffect(() => {
        const controller = new AbortController();
        if (!isChangingError) {
            fetchData(controller)
        }

        return () => controller.abort();
    }, [region, errorCount, seed, page, isChangingError])

    useEffect(() => {
        const observer = new IntersectionObserver(
            entries => {
                if (entries[0].isIntersecting && !loading && !loadingError) {
                    setPage(prevPage => prevPage + 1)
                }
            },
            { threshold: 1 }
        )

        if (observerTarget.current) {
            observer.observe(observerTarget.current)
        }

        return () => {
            if (observerTarget.current) {
                observer.unobserve(observerTarget.current)
            }
        }
    }, [loading])

    const fetchData = async (controller: AbortController) => {
        
        setLoading(true)
        fetch(`/api/fakedata?region=${region}&errorCount=${errorCount}&seed=${seed}&page=${page}`, { signal: controller?.signal } ).then(async response => {
                if (response.ok) {
                    const newData = await response.json()
                    setUserData(prevData => [...prevData, ...newData])
                    setLoading(false)
                    setLoadingError(false)
                } else {
                    setLoadingError(true)
                }
        }).catch(err => {
            if (err?.name != 'AbortError') {
                setLoadingError(true)
            } 
        })
    }

    const handleRegionChange = (value: Region) => {
        setRegion(value)
        setUserData([])
        setPage(0)
    }

    const handleErrorCountChange = (value: number[]) => {
        setIsChangingError(true);
        setErrorCount(value[0])
    }

    const handleErrorCountCommit = (value: number[]) => {
        setIsChangingError(false);
        setErrorCount(value[0])
        setUserData([])
        setPage(0)
    }

    const handleErrorInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = parseFloat(e.target.value)
        if (!isNaN(value) && value >= 0 && value <= 1000) {
            setErrorCount(value)
            setUserData([])
            setPage(0)
        }
    }

    const handleSeedChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setSeed(e.target.value)
        setUserData([])
        setPage(0)
    }

    const generateRandomSeed = () => {
        const randomSeed = Math.random().toString(36).substring(2, 15)
        setSeed(randomSeed)
        setUserData([])
        setPage(0)
    }

    const reloadData = () => {
        setUserData([])
        setPage(0)
        fetchData(new AbortController())
    }

    const exportToCsv = async () => {
        try {
            const response = await fetch(`/api/fakedata/export?region=${region}&errorCount=${errorCount}&seed=${seed}&toPage=${page}`)
            const blob = await response.blob()
            const url = window.URL.createObjectURL(blob)
            const a = document.createElement('a')
            a.style.display = 'none'
            a.href = url
            a.download = 'fake_user_data.csv'
            document.body.appendChild(a)
            a.click()
            window.URL.revokeObjectURL(url)
        } catch (error) {
            console.error('Error exporting data:', error)
        }
    }

    return (
        <div className="container mx-auto p-4">
            <h1 className="text-2xl font-bold mb-4">Fake Data Generator</h1>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                <div>
                    <Label htmlFor="region">Region</Label>
                    <Select onValueChange={handleRegionChange} value={region}>
                        <SelectTrigger id="region">
                            <SelectValue placeholder="Select region" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="USA">USA (English)</SelectItem>
                            <SelectItem value="Poland">Poland (Polish)</SelectItem>
                            <SelectItem value="Ukraine">Ukraine (Ukrainian)</SelectItem>
                        </SelectContent>
                    </Select>
                </div>
                <div>
                    <Label htmlFor="errorCount">Error Count</Label>
                    <div className="flex items-center space-x-2">
                        <Slider
                            id="errorCount"
                            min={0}
                            max={10}
                            step={0.25}
                            value={[errorCount]}
                            onValueChange={handleErrorCountChange}
                            onValueCommit={handleErrorCountCommit}
                        />
                        <Input
                            type="number"
                            value={errorCount}
                            onChange={handleErrorInputChange}
                            className="w-20"
                            min={0}
                            max={1000}
                            step={1}
                        />
                    </div>
                </div>
                <div>
                    <Label htmlFor="seed">Seed</Label>
                    <div className="flex space-x-2">
                        <Input
                            id="seed"
                            value={seed}
                            onChange={handleSeedChange}
                            placeholder="Enter seed"
                        />
                        <Button onClick={generateRandomSeed}>Random</Button>
                    </div>
                </div>
            </div>
            <Button onClick={exportToCsv} className="mb-4">Export to CSV</Button>
            {!loadingError
                ? (<ScrollArea>
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead>Number</TableHead>
                                <TableHead>ID</TableHead>
                                <TableHead>Full Name</TableHead>
                                <TableHead>Address</TableHead>
                                <TableHead>Phone</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {userData.map((user, index) => (
                                <TableRow key={index}>
                                    <TableCell>{user.number}</TableCell>
                                    <TableCell>{user.id}</TableCell>
                                    <TableCell>{user.fullName}</TableCell>
                                    <TableCell>{user.address}</TableCell>
                                    <TableCell>{user.phone}</TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                    {loading && <p>Loading...</p>}
                    <div ref={observerTarget} />
                </ScrollArea>)
                : (<div>
                    <h1 className="text-2xl font-bold mb-4">Loading error</h1>
                    <Button onClick={reloadData}>Reload</Button>
                </div>
                )
            }
        </div>
    )
}